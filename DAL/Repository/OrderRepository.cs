using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<User> _userRepository;
    
    public OrderRepository(IRepository<Order> orderRepository, VerdantTechDbContext dbContext, IRepository<User> userRepository, IOrderDetailRepository orderDetailRepository)
    {
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _userRepository = userRepository;
        _orderDetailRepository = orderDetailRepository;
    }
    
    public async Task<Order> CreateOrderWithTransactionAsync(Order order, List<OrderDetail> orderDetails, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;
            var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);
            
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.OrderId = createdOrder.Id;
                await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);
            }
            await transaction.CommitAsync(cancellationToken);
            return createdOrder;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Order> UpdateOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            order.UpdatedAt = DateTime.UtcNow;
            var updatedOrder = await _orderRepository.UpdateAsync(order, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return updatedOrder;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<bool> DeleteOrderWithTransactionAsync(Order order, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var updatedOrder = await _orderRepository.DeleteAsync(order, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedOrder;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Order?> GetOrderByIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetWithRelationsAsync(
            o => o.Id == orderId, 
            true,
            query => query.Include(o => o.OrderDetails)
                .ThenInclude(o => o.Product)
                .Include(o => o.Address),
            cancellationToken);
    }
    
    public async Task<bool> FindUserExistAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.AnyAsync(o => o.Id == userId, cancellationToken);
    }
}