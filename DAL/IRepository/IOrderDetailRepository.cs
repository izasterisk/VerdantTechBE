using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default);
    Task<bool> IsSerialRequiredByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
    Task<ProductSerial> GetProductSerialAsync(ulong productId, string serialNumber, string lotNumber, CancellationToken cancellationToken = default);
}