using BLL.DTO.Dashboard.VendorDashboard;

namespace BLL.Interfaces;

public interface IVendorDashboardService
{
    Task<VendorOverviewDTO> GetOverviewAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<VendorRevenueDTO> GetRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<VendorDailyRevenueDTO> GetDailyRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<VendorMonthlyRevenueDTO> GetMonthlyRevenueAsync(ulong vendorId, int year, CancellationToken cancellationToken = default);
    Task<VendorOrderStatisticsDTO> GetOrderStatisticsAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<VendorProductStatisticsDTO> GetProductStatisticsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<VendorBestSellingProductsDTO> GetBestSellingProductsAsync(ulong vendorId, DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default);
    Task<VendorProductRatingsDTO> GetProductRatingsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<VendorWalletStatisticsDTO> GetWalletStatisticsAsync(ulong vendorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<VendorPendingItemsDTO> GetPendingItemsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportTransactionHistoryAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

