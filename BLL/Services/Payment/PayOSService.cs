using BLL.DTO.Payment.PayOS;
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
    
    public PayOSService(IPayOSApiClient payOSApiClient, IOrderRepository orderRepository)
    {
        _payOSApiClient = payOSApiClient;
        _orderRepository = orderRepository;
    }
    
    public async Task<CreatePaymentResult> CreatePaymentLinkAsync(ulong orderId, CreatePaymentDataDTO dto)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null || order.Status != OrderStatus.Paid)
        {
            throw new ArgumentException($"Đơn hàng với ID {orderId} không tồn tại hoặc không khả dụng để thanh toán.");
        }
        PaymentData paymentData = new PaymentData(
            orderCode: checked((long)order.Id),
            amount: (int)Math.Round(order.TotalAmount, MidpointRounding.AwayFromZero),
            description: dto.Description,
            items: dto.Items.Select(item => new ItemData(
                name: item.Name,
                quantity: item.Quantity,
                price: item.Price
            )).ToList(),
            cancelUrl: dto.CancelUrl,
            returnUrl: dto.ReturnUrl,
            signature: dto.Signature,
            buyerName: dto.BuyerName,
            buyerEmail: dto.BuyerEmail,
            buyerPhone: dto.BuyerPhone,
            buyerAddress: dto.BuyerAddress,
            expiredAt: dto.ExpiredAt
        );
        return await _payOSApiClient.CreatePaymentLinkAsync(paymentData);
    }
}