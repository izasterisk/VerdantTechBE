using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderRepository
{
    Task<Order> CreateOrderWithTransactionAsync(Order order, List<OrderDetail> orderDetails, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(ulong orderId, CancellationToken cancellationToken = default);
    Task<Order> UpdateOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> FindUserExistAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateAddressBelongsToUserAsync(ulong addressId, ulong userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllOrdersByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}