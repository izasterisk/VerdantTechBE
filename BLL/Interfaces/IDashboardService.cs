using BLL.DTO.Dashboard;

namespace BLL.Interfaces;

public interface IDashboardService
{
    Task<RevenueByTimeRangeResponseDTO> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<OrderStatisticsResponseDTO> GetOrderStatisticsByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<QueueStatisticsResponseDTO> GetQueueStatisticsAsync(CancellationToken cancellationToken = default);
    Task<RevenueLast7DaysResponseDTO> GetRevenueLast7DaysAsync(CancellationToken cancellationToken = default);
}
