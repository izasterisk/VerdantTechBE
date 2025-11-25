using BLL.DTO.Dashboard;
using BLL.Interfaces;
using DAL.IRepository;

namespace BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<RevenueByTimeRangeResponseDTO> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var revenue = await _dashboardRepository.GetRevenueByTimeRangeAsync(from, to, cancellationToken);
        return new RevenueByTimeRangeResponseDTO
        {
            From = from,
            To = to,
            Revenue = revenue
        };
    }

    public async Task<OrderStatisticsResponseDTO> GetOrderStatisticsByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var (total, paid, shipped, cancelled, delivered, refunded) = 
            await _dashboardRepository.GetNumberOfOrdersByTimeRangeAsync(from, to, cancellationToken);
        
        return new OrderStatisticsResponseDTO
        {
            From = from,
            To = to,
            Total = total,
            Paid = paid,
            Shipped = shipped,
            Cancelled = cancelled,
            Delivered = delivered,
            Refunded = refunded
        };
    }

    public async Task<QueueStatisticsResponseDTO> GetQueueStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var (vendorProfile, productRegistration, vendorCertificate, productCertificate, request) = 
            await _dashboardRepository.GetNumberOfQueuesAsync(cancellationToken);
        
        return new QueueStatisticsResponseDTO
        {
            VendorProfile = vendorProfile,
            ProductRegistration = productRegistration,
            VendorCertificate = vendorCertificate,
            ProductCertificate = productCertificate,
            Request = request
        };
    }

    public async Task<RevenueLast7DaysResponseDTO> GetRevenueLast7DaysAsync(CancellationToken cancellationToken = default)
    {
        var revenues = await _dashboardRepository.GetRevenueLast7DaysAsync(cancellationToken);
        
        var dailyRevenues = revenues
            .OrderBy(r => r.Key)
            .Select(r => new DailyRevenueDTO
            {
                Date = r.Key,
                Revenue = r.Value
            })
            .ToList();

        return new RevenueLast7DaysResponseDTO
        {
            From = dailyRevenues.First().Date,
            To = dailyRevenues.Last().Date,
            TotalRevenue = dailyRevenues.Sum(r => r.Revenue),
            DailyRevenues = dailyRevenues
        };
    }
}
