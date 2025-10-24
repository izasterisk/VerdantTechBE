using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderRepository
{
    Task<Order> CreateOrderWithTransactionAsync(Order order, List<OrderDetail> orderDetails, List<Product> products,
        CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(ulong orderId, CancellationToken cancellationToken = default);
    Task<Order> UpdateOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default);
    Task<User?> GetActiveUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateAddressBelongsToUserAsync(ulong addressId, ulong userId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllOrdersByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<(List<Order>, int totalCount)> GetAllOrdersAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<Product?> GetActiveProductByIdAsync(ulong productId, CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetProductImagesByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
}