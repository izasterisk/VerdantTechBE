using BLL.DTO.Dashboard;
using BLL.Interfaces;
using DAL.Data;
using DAL.IRepository;
using BLL.DTO.Order;

namespace BLL.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IUserRepository _userRepository;
    
    public DashboardService(IDashboardRepository dashboardRepository, IUserRepository userRepository)
    {
        _dashboardRepository = dashboardRepository;
        _userRepository = userRepository;
    }

    public async Task<RevenueByTimeRangeResponseDTO> GetRevenueByTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ulong? id = null;
        var vendor = await _userRepository.GetVerifiedAndActiveUserByIdAsync(vendorId, cancellationToken);
        if (vendor.Role == UserRole.Vendor)
            id = vendorId;
        
        var revenue = await _dashboardRepository.GetRevenueByTimeRangeAsync(from, to, id, cancellationToken);
        return new RevenueByTimeRangeResponseDTO
        {
            From = from,
            To = to,
            Revenue = revenue
        };
    }

    public async Task<Top5BestSellingProductsResponseDTO> GetTop5BestSellingProductsByTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ulong? id = null;
        var vendor = await _userRepository.GetVerifiedAndActiveUserByIdAsync(vendorId, cancellationToken);
        if (vendor.Role == UserRole.Vendor)
            id = vendorId;
        
        var results = await _dashboardRepository.GetTop5BestSellingProductsByTimeRangeAsync(from, to, id, cancellationToken);
        var products = results.Select(r => new Top5BestSellingProductsDTO
        {
            SoldQuantity = r.soldQuantity,
            Product = new ProductResponseDTO
            {
                Id = r.product.Id,
                ProductCode = r.product.ProductCode,
                ProductName = r.product.ProductName,
                Slug = r.product.Slug,
                Description = r.product.Description,
                UnitPrice = r.product.UnitPrice,
                WarrantyMonths = r.product.WarrantyMonths,
                Specifications = r.product.Specifications,
                DimensionsCm = r.product.DimensionsCm,
                Images = r.image != null 
                    ? new List<ProductImageResponseDTO> 
                    { 
                        new ProductImageResponseDTO 
                        { 
                            ImageUrl = r.image.ImageUrl, 
                            SortOrder = r.image.SortOrder 
                        } 
                    }
                    : new List<ProductImageResponseDTO>()
            }
        }).ToList();
        return new Top5BestSellingProductsResponseDTO
        {
            From = from,
            To = to,
            Products = products
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
