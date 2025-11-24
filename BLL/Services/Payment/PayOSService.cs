using AutoMapper;
using BLL.DTO.Payment.PayOS;
using BLL.DTO.Transaction;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.IRepository;
using Net.payOS.Types;

namespace BLL.Services.Payment;

public class PayOSService : IPayOSService
{
    private readonly IPayOSApiClient _payOSApiClient;
    private readonly IOrderRepository _orderRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationService _notificationService;
    private readonly string _frontEndUrl;
    private readonly IMapper _mapper;
    
    public PayOSService(IPayOSApiClient payOSApiClient, IOrderRepository orderRepository, 
        ITransactionRepository transactionRepository, IPaymentRepository paymentRepository, 
        INotificationService notificationService, IMapper mapper)
    {
        _payOSApiClient = payOSApiClient;
        _orderRepository = orderRepository;
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
        _notificationService = notificationService;
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
        ArgumentNullException.ThrowIfNull(webhookBody, $"{nameof(webhookBody)} rỗng.");
        WebhookData webhookData = _payOSApiClient.VerifyWebhookData(webhookBody);
        
        var transaction = await _transactionRepository.GetTransactionForPaymentByGatewayPaymentIdAsync(webhookData.orderCode.ToString(), cancellationToken);
        if (transaction.Order == null || transaction.Payment == null)
            throw new KeyNotFoundException("Giao dịch này không liên kết với đơn hàng nào.");

        string title; string message;
        if (webhookData.code == "00" || webhookData.desc == "Thành công")
        {
            transaction.Order.Status = OrderStatus.Paid;
            
            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedAt = DateTime.UtcNow;
            
            await _paymentRepository.UpdateFullPaymentWithTransactionAsync(transaction.Payment, transaction.Order, transaction, cancellationToken);
            title = "Thanh toán thành công";
            message = $"Đơn hàng #{transaction.Order.Id} đã được thanh toán thành công qua PayOS với số tiền {webhookData.amount:N0} VND.";
        }
        else
        {
            transaction.Status = TransactionStatus.Failed;
            await _paymentRepository.UpdateFullPaymentWithTransactionAsync(transaction.Payment, transaction.Order, transaction, cancellationToken);
            title = "Thanh toán thất bại";
            message = $"Đơn hàng #{transaction.Order.Id} thanh toán thất bại.";
        }
        await _notificationService.CreateAndSendNotificationAsync(
            userId: transaction.UserId,
            title: title,
            message: message,
            referenceType: NotificationReferenceType.Payment,
            referenceId: transaction.Payment.Id,
            cancellationToken: cancellationToken
        );
        return webhookData;
    }
    
    public async Task ConfirmWebhookAsync(ConfirmWebhookDTO dto, CancellationToken cancellationToken = default)
    {
        await _payOSApiClient.ConfirmWebhookAsync(dto.WebhookUrl);
    }
    
    public async Task<TransactionResponseDTO> GetPaymentLinkInformationAsync(ulong transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _paymentRepository.GetPaymentWithRelationByTransactionIdAsync(transactionId, cancellationToken);
        if(transaction.TransactionType != TransactionType.PaymentIn)
            throw new InvalidOperationException("Giao dịch này không phải là giao dịch thanh toán.");
        
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