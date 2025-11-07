using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail);
    Task<ulong> GetRootProductCategoryIdByProductIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<ulong?> ValidateIdentifyNumberAsync(ulong productId, string? serialNumber, string? lotNumber, CancellationToken cancellationToken = default);

}