using AutoMapper;
using BLL.DTO.Payment.PayOS;
using BLL.DTO.Transaction;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.IRepository;
using Microsoft.Extensions.Logging;
using Net.payOS.Types;

namespace BLL.Services.Payment;

public class PayOSService : IPayOSService
{
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly IOrderRepository _orderRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PayOSService> _logger;
    private readonly string _frontEndUrl;
    private readonly IMapper _mapper;
    
    public PayOSService(IPayOSApiClient payOSApiClient, IOrderRepository orderRepository, 
        ITransactionRepository transactionRepository, IPaymentRepository paymentRepository, 
        INotificationService notificationService, ILogger<PayOSService> logger, IMapper mapper)
    {
        _payOSApiClient = payOSApiClient;
        _orderRepository = orderRepository;
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
        _notificationService = notificationService;
        _logger = logger;
        _mapper = mapper;
        _frontEndUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ??
                       throw new InvalidOperationException("FRONTEND_URL không được cấu hình trong .env file");
    }
    
    public async Task<CreatePaymentResult> CreatePaymentLinkAsync(ulong orderId, CreatePaymentDataDTO dto, CancellationToken cancellationToken = default)
    {
        var cancelUrl = $"{_frontEndUrl}/payos/cancel";
        var returnUrl = $"{_frontEndUrl}/payos/return";
        
        var order = await _orderRepository.GetOrderWithRelationsByIdAsync(orderId, cancellationToken);
        if (order == null || order.Status != OrderStatus.Pending || order.OrderPaymentMethod == OrderPaymentMethod.COD)
        {
            throw new ArgumentException($"Đơn hàng với ID {orderId} không tồn tại hoặc không khả dụng để thanh toán.");
        }
        List<ItemData> items = new();
        foreach (var orderDetail in order.OrderDetails)
        {
            items.Add(new ItemData(
                name: orderDetail.Product.ProductName,
                quantity: orderDetail.Quantity,
                price: (int)Math.Round(orderDetail.UnitPrice, MidpointRounding.AwayFromZero)
            ));
        }
        long orderCode;
        while (true)
        {
            orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 100 + Random.Shared.Next(0, 99);
            try
            {
                await _payOSApiClient.GetPaymentLinkInformationAsync(orderCode);
            }
            catch
            {
                break;
            }
        }
        PaymentData paymentData = new PaymentData(
            orderCode: orderCode,
            amount: (int)Math.Round(order.TotalAmount, MidpointRounding.AwayFromZero),
            description: dto.Description,
            items: items,
            cancelUrl: cancelUrl,
            returnUrl: returnUrl
        );
        var createdPayment = await _payOSApiClient.CreatePaymentLinkAsync(paymentData);
        var payment = new DAL.Data.Models.Payment
        {
            PaymentMethod = PaymentMethod.Payos,
            PaymentGateway = PaymentGateway.Payos,
            GatewayResponse = new Dictionary<string, object>
            {
                { "bin", createdPayment.bin },
                { "accountNumber", createdPayment.accountNumber },
                { "amount", createdPayment.amount },
                { "description", createdPayment.description },
                { "orderCode", createdPayment.orderCode },
                { "currency", createdPayment.currency },
                { "paymentLinkId", createdPayment.paymentLinkId },
                { "status", createdPayment.status },
                { "expiredAt", createdPayment.expiredAt ?? 0 },
                { "checkoutUrl", createdPayment.checkoutUrl },
            }
        };
        var transaction = new DAL.Data.Models.Transaction
        {
            TransactionType = TransactionType.PaymentIn,
            Amount = paymentData.amount,
            Currency = "VND",
            UserId = order.CustomerId,
            OrderId = order.Id,
            // BankAccountId
            Status = TransactionStatus.Pending,
            Note = $"Thanh toán đơn hàng #{order.Id} qua PayOS",
            GatewayPaymentId = orderCode.ToString(),
            CreatedBy = order.CustomerId
        };
        await _paymentRepository.CreatePaymentWithTransactionAsync(payment, transaction, cancellationToken);
        return createdPayment;
    }
    
    public async Task<WebhookData> HandlePayOSWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PayOS Webhook] Bắt đầu xử lý webhook từ PayOS");
        
        ArgumentNullException.ThrowIfNull(webhookBody, $"{nameof(webhookBody)} rỗng.");
        
        _logger.LogInformation("[PayOS Webhook] Đang verify webhook data...");
        WebhookData webhookData = _payOSApiClient.VerifyWebhookData(webhookBody);
        
        _logger.LogInformation("[PayOS Webhook] Webhook data verified - OrderCode: {OrderCode}, Code: {Code}, Desc: {Desc}, Amount: {Amount}, CounterAccountName: {CounterAccountName}",
            webhookData.orderCode, webhookData.code, webhookData.desc, webhookData.amount, webhookData.counterAccountName ?? "null");
        
        _logger.LogInformation("[PayOS Webhook] Đang tìm transaction với GatewayPaymentId: {GatewayPaymentId}", webhookData.orderCode.ToString());
        var transaction = await _transactionRepository.GetTransactionForPaymentByGatewayPaymentIdAsync(webhookData.orderCode.ToString(), cancellationToken);
        
        if (transaction.Order == null || transaction.Payment == null)
        {
            _logger.LogError("[PayOS Webhook] Transaction không có Order hoặc Payment - TransactionId: {TransactionId}, OrderNull: {OrderNull}, PaymentNull: {PaymentNull}",
                transaction.Id, transaction.Order == null, transaction.Payment == null);
            throw new KeyNotFoundException("Giao dịch này không liên kết với đơn hàng nào.");
        }
        
        _logger.LogInformation("[PayOS Webhook] Tìm thấy transaction - TransactionId: {TransactionId}, OrderId: {OrderId}, PaymentId: {PaymentId}, CurrentTransactionStatus: {CurrentTransactionStatus}, CurrentOrderStatus: {CurrentOrderStatus}",
            transaction.Id, transaction.Order.Id, transaction.Payment.Id, transaction.Status, transaction.Order.Status);

        string title; string message; string? customerName = null;
        if (webhookData.code == "00" || webhookData.desc == "Thành công")
        {
            _logger.LogInformation("[PayOS Webhook] Thanh toán thành công - Đang cập nhật trạng thái...");
            
            transaction.Order.Status = OrderStatus.Paid;
            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedAt = DateTime.UtcNow;
            if(webhookData.counterAccountName != null)
                customerName = webhookData.counterAccountName;
            
            _logger.LogInformation("[PayOS Webhook] Trạng thái mới - OrderStatus: {OrderStatus}, TransactionStatus: {TransactionStatus}, ProcessedAt: {ProcessedAt}, CustomerName: {CustomerName}",
                transaction.Order.Status, transaction.Status, transaction.ProcessedAt, customerName ?? "null");
            
            title = "Thanh toán thành công";
            message = $"Đơn hàng #{transaction.Order.Id} đã được thanh toán thành công qua PayOS với số tiền {webhookData.amount:N0} VND.";
        }
        else
        {
            _logger.LogWarning("[PayOS Webhook] Thanh toán thất bại - Code: {Code}, Desc: {Desc}", webhookData.code, webhookData.desc);
            transaction.Status = TransactionStatus.Failed;
            title = "Thanh toán thất bại";
            message = $"Đơn hàng #{transaction.Order.Id} thanh toán thất bại.";
        }
        
        _logger.LogInformation("[PayOS Webhook] Đang lưu vào database - PaymentId: {PaymentId}, OrderId: {OrderId}, TransactionId: {TransactionId}",
            transaction.Payment.Id, transaction.Order.Id, transaction.Id);
        
        try
        {
            await _paymentRepository.UpdateFullPaymentWithTransactionAsync(transaction.Payment, transaction.Order, transaction, customerName, cancellationToken);
            _logger.LogInformation("[PayOS Webhook] ĐÃ LƯU THÀNH CÔNG vào database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PayOS Webhook] LỖI KHI LƯU vào database - Message: {Message}", ex.Message);
            throw;
        }
        
        _logger.LogInformation("[PayOS Webhook] Đang gửi notification tới UserId: {UserId}", transaction.UserId);
        await _notificationService.CreateAndSendNotificationAsync(
            userId: transaction.UserId,
            title: title,
            message: message,
            referenceType: NotificationReferenceType.Payment,
            referenceId: transaction.Payment.Id,
            cancellationToken: cancellationToken
        );
        
        _logger.LogInformation("[PayOS Webhook] Hoàn thành xử lý webhook - OrderCode: {OrderCode}", webhookData.orderCode);
        return webhookData;
    }
    
    public async Task ConfirmWebhookAsync(ConfirmWebhookDTO dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PayOS Webhook] Bắt đầu confirm webhook URL - WebhookUrl: {WebhookUrl}", dto.WebhookUrl);
        
        try
        {
            await _payOSApiClient.ConfirmWebhookAsync(dto.WebhookUrl);
            _logger.LogInformation("[PayOS Webhook] ĐÃ CONFIRM THÀNH CÔNG webhook URL: {WebhookUrl}", dto.WebhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PayOS Webhook] LỖI KHI CONFIRM webhook URL: {WebhookUrl} - Message: {Message}, StackTrace: {StackTrace}", 
                dto.WebhookUrl, ex.Message, ex.StackTrace);
            throw;
        }
    }
    
    public async Task<TransactionResponseDTO> GetPaymentLinkInformationAsync(ulong transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentRepository.GetPaymentWithRelationByTransactionIdAsync(transactionId, cancellationToken);
        // if(transaction.TransactionType != TransactionType.PaymentIn)
        //     throw new InvalidOperationException("Giao dịch này không phải là giao dịch thanh toán.");
        
        var response = _mapper.Map<TransactionResponseDTO>(transaction);
        
        if (!string.IsNullOrEmpty(transaction.GatewayPaymentId))
        {
            if (!long.TryParse(transaction.GatewayPaymentId, out long orderCode))
                throw new FormatException($"GatewayPaymentId '{transaction.GatewayPaymentId}' không phải là định dạng hợp lệ.");
            response.Payment.PaymentLinkInformation = await _payOSApiClient.GetPaymentLinkInformationAsync(orderCode);
        }
        
        return response;
    }
    
    // public async Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode)
    // {
    //     return await _payOSApiClient.CancelPaymentLinkAsync(orderCode);
    // }
}