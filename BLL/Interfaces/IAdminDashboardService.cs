using BLL.DTO.Dashboard.Dashboard;
using BLL.DTO.Dashboard;
using BLL.DTO;

namespace BLL.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminOverviewDTO> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<AdminRevenueDTO> GetRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<AdminDailyRevenueDTO> GetDailyRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<AdminMonthlyRevenueDTO> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default);
    Task<AdminOrderStatisticsDTO> GetOrderStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<AdminUserStatisticsDTO> GetUserStatisticsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<AdminProductStatisticsDTO> GetProductStatisticsAsync(CancellationToken cancellationToken = default);
    Task<AdminBestSellingProductsDTO> GetBestSellingProductsAsync(DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default);
    Task<AdminVendorStatisticsDTO> GetVendorStatisticsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<AdminTopVendorsDTO> GetTopVendorsAsync(DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default);
    Task<AdminTransactionStatisticsDTO> GetTransactionStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<AdminQueueStatisticsDTO> GetQueueStatisticsAsync(CancellationToken cancellationToken = default);
    Task<PagedResponse<TransactionExportDTO>> GetTransactionsAsync(DateOnly from, DateOnly to, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<byte[]> ExportTransactionHistoryAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

