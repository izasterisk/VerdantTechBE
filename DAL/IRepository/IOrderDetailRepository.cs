using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default);
    Task<List<OrderDetail>> GetListedOrderDetailsByIdAsync(List<ulong> orderDetailIds, CancellationToken cancellationToken = default);
    // Task<ulong> GetRootProductCategoryIdByProductIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<ulong?> ValidateIdentifyNumberAsync(ulong productId, string? serialNumber, string? lotNumber, CancellationToken cancellationToken = default);
}