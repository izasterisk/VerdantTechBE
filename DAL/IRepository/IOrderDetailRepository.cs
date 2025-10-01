using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail);
    Task<OrderDetail> UpdateOrderDetailAsync(OrderDetail orderDetail);
    Task<bool> DeleteOrderDetailAsync(OrderDetail orderDetail);
    Task<OrderDetail?> GetOrderDetailByIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default);
    Task<bool> HasNoOrderDetailLeftAsync(ulong orderId, CancellationToken cancellationToken = default);
}