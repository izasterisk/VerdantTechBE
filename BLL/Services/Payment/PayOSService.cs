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
    private readonly string _frontEndUrl;
    private readonly IMapper _mapper;
    
    public PayOSService(IPayOSApiClient payOSApiClient, IOrderRepository orderRepository, 
        ITransactionRepository transactionRepository, IPaymentRepository paymentRepository, IMapper mapper)
    {
        _payOSApiClient = payOSApiClient;
        _orderRepository = orderRepository;
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
        _mapper = mapper;
        _frontEndUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ??
                       throw new InvalidOperationException("FRONTEND_URL không được cấu hình trong .env file");
    }
    
    public async Task<CreatePaymentResult> CreatePaymentLinkAsync(ulong orderId, CreatePaymentDataDTO dto)
    {
        var cancelUrl = $"{_frontEndUrl}/payos/cancel";
        var returnUrl = $"{_frontEndUrl}/payos/return";
        
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
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
        var payment = new PaymentResponseDTO
        {
            OrderId = order.Id,
            PaymentMethod = PaymentMethod.Payos,
            PaymentGateway = PaymentGateway.Payos,
            GatewayPaymentId = createdPayment.orderCode.ToString(),
            Amount = createdPayment.amount,
            Status = PaymentStatus.Pending,
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
        await _paymentRepository.CreatePaymentWithTransactionAsync(_mapper.Map<DAL.Data.Models.Payment>(payment));
        return createdPayment;
    }
    
    public async Task<WebhookData> HandlePayOSWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookBody, $"{nameof(webhookBody)} rỗng.");
        WebhookData webhookData = _payOSApiClient.VerifyWebhookData(webhookBody);
        
        var payment = await _paymentRepository.GetPaymentByGatewayPaymentIdAsync(webhookData.orderCode.ToString(), cancellationToken);
        if (payment == null)
            throw new KeyNotFoundException($"Không tìm thấy thanh toán với mã đơn hàng: {webhookData.orderCode}");

        if (webhookData.code == "00" || webhookData.desc == "Thành công")
        {
            payment.Status = PaymentStatus.Completed;
            payment.Order.Status = OrderStatus.Paid;
            var transaction = new TransactionResponseDTO
            {
                TransactionType = TransactionType.PaymentIn,
                Amount = webhookData.amount,
                Currency = webhookData.currency,
                OrderId = payment.OrderId,
                UserId = payment.Order.CustomerId,
                Status = TransactionStatus.Completed,
                Note = $"Thanh toán đơn hàng #{payment.OrderId} qua PayOS",
                GatewayPaymentId = webhookData.orderCode.ToString(),
                CreatedBy = payment.Order.CustomerId,
                CompletedAt = DateTime.UtcNow
            };
            await _paymentRepository.UpdateFullPaymentWithTransactionAsync(payment, payment.Order, 
                _mapper.Map<DAL.Data.Models.Transaction>(transaction), cancellationToken);
        }
        return webhookData;
    }
    
    public async Task ConfirmWebhookAsync(ConfirmWebhookDTO dto, CancellationToken cancellationToken = default)
    {
        await _payOSApiClient.ConfirmWebhookAsync(dto.WebhookUrl);
    }
    
    // public async Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode)
    // {
    //     return await _payOSApiClient.GetPaymentLinkInformationAsync(orderCode);
    // }
    
    // public async Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode)
    // {
    //     return await _payOSApiClient.CancelPaymentLinkAsync(orderCode);
    // }
}