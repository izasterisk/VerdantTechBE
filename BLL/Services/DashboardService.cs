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

    public async Task<RevenueLast7DaysResponseDTO> GetRevenueLast7DaysAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        ulong? id = null;
        var vendor = await _userRepository.GetVerifiedAndActiveUserByIdAsync(vendorId, cancellationToken);
        if (vendor.Role == UserRole.Vendor)
            id = vendorId;
        
        var revenues = await _dashboardRepository.GetRevenueLast7DaysAsync(id, cancellationToken);
        
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
    
    public async Task<ProductsRatingDTO> GetAverageRatingsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var vendor = await _userRepository.GetVerifiedAndActiveUserByIdAsync(vendorId, cancellationToken);
        if (vendor.Role != UserRole.Vendor)
            throw new UnauthorizedAccessException("Chỉ nhà cung cấp mới có thể truy cập thông tin này.");
        
        var response = new ProductsRatingDTO();
        var ratings = await _dashboardRepository.GetAverageRatingsByVendorIdAsync(vendorId, cancellationToken);
        if (ratings.Count > 0)
        {
            var top3HighestList = ratings.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var top3LowestList = ratings.OrderBy(x => x.Value).Take(3).Select(x => x.Key).ToList();
            var productIds = top3HighestList.Concat(top3LowestList).Distinct().ToList();
            
            var productsDict = await _dashboardRepository.GetProductsWithImagesByListAsync(productIds, cancellationToken);
            var products = new List<ProductResponseDTO>();
            foreach (var product in productsDict)
            {
                products.Add(new ProductResponseDTO
                {
                    Id = product.Key.Id,
                    ProductCode = product.Key.ProductCode,
                    ProductName = product.Key.ProductName,
                    Slug = product.Key.Slug,
                    Description = product.Key.Description,
                    UnitPrice = product.Key.UnitPrice,
                    WarrantyMonths = product.Key.WarrantyMonths,
                    Specifications = product.Key.Specifications,
                    DimensionsCm = product.Key.DimensionsCm,
                    RatingAverage = product.Key.RatingAverage,
                    Images = product.Value.Select(img => new ProductImageResponseDTO
                    {
                        ImageUrl = img.ImageUrl,
                        SortOrder = img.SortOrder
                    }).ToList()
                });
            }
            
            response.Top3Highest = new TopProductsRatingResponseDTO
            {
                Top1 = top3HighestList.Count > 0 ? products.First(p => p.Id == top3HighestList[0]) : null!,
                Top2 = top3HighestList.Count > 1 ? products.First(p => p.Id == top3HighestList[1]) : null!,
                Top3 = top3HighestList.Count > 2 ? products.First(p => p.Id == top3HighestList[2]) : null!
            };
            
            response.Top3Lowest = new WorstProductsRatingResponseDTO
            {
                Top1 = top3LowestList.Count > 0 ? products.First(p => p.Id == top3LowestList[0]) : null!,
                Top2 = top3LowestList.Count > 1 ? products.First(p => p.Id == top3LowestList[1]) : null!,
                Top3 = top3LowestList.Count > 2 ? products.First(p => p.Id == top3LowestList[2]) : null!
            };
            response.AverageRatingOfVendor = ratings.Values.Average();
        }
        return response;
    }
}  
    