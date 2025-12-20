using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserAddress> _userAddressRepository;
    private readonly IRepository<FarmProfile> _farmProfileRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<MediaLink> _mediaLinkRepository;
    
    public OrderRepository(IOrderDetailRepository orderDetailRepository,
        IRepository<Order> orderRepository, VerdantTechDbContext dbContext, IRepository<User> userRepository,
        IRepository<UserAddress> userAddressRepository, IRepository<FarmProfile> farmProfileRepository,
        IRepository<Product> productRepository, IRepository<MediaLink> mediaLinkRepository)
    {
        _orderDetailRepository = orderDetailRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _userRepository = userRepository;
        _userAddressRepository = userAddressRepository;
        _farmProfileRepository = farmProfileRepository;
        _productRepository = productRepository;
        _mediaLinkRepository = mediaLinkRepository;
    }
    
    public async Task<Order> CreateOrderWithTransactionAsync(Order order, List<OrderDetail> orderDetails, List<Product> products, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;
            order.IsWalletCredited = false;
            var createdOrder = await _orderRepository.CreateAsync(order, cancellationToken);
            foreach (var product in products)
            {
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.OrderId = createdOrder.Id;
                await _orderDetailRepository.CreateOrderDetailAsync(orderDetail, cancellationToken);
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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
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
    
    public async Task<Order> UpdateOrderWithProductsTransactionAsync(Order order, List<Product> productsToUpdate, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var product in productsToUpdate)
            {
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
            
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
    
    public async Task<Order?> GetOrderWithRelationsByIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetWithRelationsAsync(
            o => o.Id == orderId, 
            true,
            query => query.Include(o => o.OrderDetails)
                .ThenInclude(o => o.Product),
            cancellationToken);
    }
    
    public async Task<(List<Order>, int totalCount)> GetAllOrdersByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            o => o.CustomerId == userId, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(o => o.CreatedAt),
            includeFunc: query => query.Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Address)
                .Include(o => o.Customer),
            cancellationToken
        );
    }
    
    public async Task<(List<Order>, int totalCount)> GetAllOrdersAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<Order, bool>> filter = o => true;
        
        // Apply status filter if provided
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                filter = o => o.Status == orderStatus;
            }
        }
        return await _orderRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(o => o.CreatedAt),
            includeFunc: query => query.Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Address)
                .Include(o => o.Customer),
            cancellationToken
        );
    }
    
    public async Task<User?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAsync(o => o.Id == userId, true, cancellationToken);
    }
    
    public async Task<bool> ValidateAddressBelongsToUserAsync(ulong addressId, ulong userId, CancellationToken cancellationToken = default)
    {
        var farm = await _farmProfileRepository.AnyAsync(f => f.UserId == userId && f.AddressId == addressId, cancellationToken);
        var user = await _userAddressRepository.AnyAsync(u => u.UserId == userId && u.AddressId == addressId && u.IsDeleted == false, cancellationToken);
        return farm || user;
    }

    public async Task<Product?> GetActiveProductByIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAsync(p => p.Id == productId && p.IsActive == true, true, cancellationToken);
    }
    
    public async Task<List<MediaLink>> GetProductImagesByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        var mediaLinks = await _mediaLinkRepository.GetAllByFilterAsync(m => m.OwnerId == productId && m.OwnerType == MediaOwnerType.Products, true, cancellationToken);
        return mediaLinks.OrderBy(m => m.SortOrder).ToList();
    }
}