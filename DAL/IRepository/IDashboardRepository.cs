using DAL.Data.Models;

namespace DAL.IRepository;

public interface IDashboardRepository
{
    Task<decimal> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, ulong? vendorId, CancellationToken cancellationToken = default);
    Task<List<(Product product, MediaLink? image, int soldQuantity)>> GetTop5BestSellingProductsByTimeRangeAsync(DateOnly from, DateOnly to, ulong? vendorId, CancellationToken cancellationToken = default);
    Task<(int Total, int Paid, int Shipped, int Cancelled, int Delivered, int Refunded)> 
        GetNumberOfOrdersByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int VendorProfile, int ProductRegistration, int VendorCertificate, int ProductCertificate, int Request)> 
        GetNumberOfQueuesAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<DateOnly, decimal>> GetRevenueLast7DaysAsync(CancellationToken cancellationToken = default);
}