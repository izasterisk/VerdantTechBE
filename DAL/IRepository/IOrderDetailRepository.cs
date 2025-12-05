using DAL.Data.Models;

namespace DAL.IRepository;

public interface IOrderDetailRepository
{
    Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail, CancellationToken cancellationToken = default);
    Task<bool> IsSerialRequiredByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
    Task<List<ProductSerial>> GetAllProductSerialToExportAsync(Dictionary<string, (string lotNumber, ulong productId)> validateSerialNumber, CancellationToken cancellationToken = default);
}