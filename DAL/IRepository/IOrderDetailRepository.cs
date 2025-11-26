using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default);
    Task<ulong> GetOrderDetailIdByOrderNProductIdAsync(ulong orderId, ulong productId, CancellationToken cancellationToken = default);
    Task<OrderDetail> GetOrderDetailWithRelationByIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default);
    Task<ProductSerial?> ValidateIdentifyNumberAsync(ulong productId, string? serialNumber, string lotNumber, CancellationToken cancellationToken = default);
}