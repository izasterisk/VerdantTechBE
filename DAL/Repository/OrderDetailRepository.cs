using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    
    public OrderDetailRepository(IRepository<OrderDetail> orderDetailRepository)
    {
        _orderDetailRepository = orderDetailRepository;
    }
    
    public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail)
    {
        orderDetail.CreatedAt = DateTime.UtcNow;
        return await _orderDetailRepository.CreateAsync(orderDetail);
    }
    
    public async Task<OrderDetail> UpdateOrderDetailAsync(OrderDetail orderDetail)
    {
        return await _orderDetailRepository.UpdateAsync(orderDetail);
    }

    public async Task<bool> DeleteOrderDetailAsync(OrderDetail orderDetail)
    {
        return await _orderDetailRepository.DeleteAsync(orderDetail);
    }
    
    public async Task<OrderDetail?> GetOrderDetailByIdAsync(ulong orderDetailId, CancellationToken cancellationToken = default)
    {
        return await _orderDetailRepository.GetAsync(o => o.Id == orderDetailId, true, cancellationToken);
    }
    
    public async Task<bool> HasNoOrderDetailLeftAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        var count = await _orderDetailRepository.CountAsync(od => od.OrderId == orderId, cancellationToken);
        return count == 0;
    }
}