using BLL.DTO.Dashboard.VendorDashboard;
using BLL.Interfaces;
using DAL.Data;
using DAL.IRepository;

namespace BLL.Services;

public class VendorDashboardService : IVendorDashboardService
{
    private readonly IVendorDashboardRepository _repository;
    private readonly IUserRepository _userRepository;

    private static readonly string[] MonthNames = 
    {
        "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
        "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
    };

    public VendorDashboardService(IVendorDashboardRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<VendorOverviewDTO> GetOverviewAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var (walletBalance, pendingCashout) = await _repository.GetWalletInfoAsync(vendorId, cancellationToken);
        var (thisMonthRevenue, lastMonthRevenue) = await _repository.GetMonthlyRevenueComparisonAsync(vendorId, cancellationToken);
        var (thisMonthOrders, lastMonthOrders) = await _repository.GetMonthlyOrderCountComparisonAsync(vendorId, cancellationToken);
        var (active, outOfStock, pendingRegistrations, pendingUpdates) = await _repository.GetProductCountsAsync(vendorId, cancellationToken);
        var (avgRating, totalReviews) = await _repository.GetRatingInfoAsync(vendorId, cancellationToken);

        return new VendorOverviewDTO
        {
            WalletBalance = walletBalance,
            PendingCashout = pendingCashout,
            TotalRevenueThisMonth = thisMonthRevenue,
            TotalRevenueLastMonth = lastMonthRevenue,
            RevenueGrowthPercent = CalculateGrowthPercent(thisMonthRevenue, lastMonthRevenue),
            TotalOrdersThisMonth = thisMonthOrders,
            TotalOrdersLastMonth = lastMonthOrders,
            OrderGrowthPercent = CalculateGrowthPercent(thisMonthOrders, lastMonthOrders),
            TotalProductsActive = active,
            TotalProductsOutOfStock = outOfStock,
            PendingProductRegistrations = pendingRegistrations,
            PendingProductUpdateRequests = pendingUpdates,
            AverageRating = avgRating,
            TotalReviews = totalReviews
        };
    }

    public async Task<VendorRevenueDTO> GetRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);
        ValidateDateRange(from, to);

        var (gross, commission, orderCount) = await _repository.GetRevenueByTimeRangeAsync(vendorId, from, to, cancellationToken);

        return new VendorRevenueDTO
        {
            From = from,
            To = to,
            TotalGrossRevenue = gross,
            TotalCommission = commission,
            TotalNetRevenue = gross - commission,
            TotalOrders = orderCount,
            AverageOrderValue = orderCount > 0 ? gross / orderCount : 0
        };
    }

    public async Task<VendorDailyRevenueDTO> GetDailyRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);
        ValidateDateRange(from, to, maxDays: 90);

        var dailyData = await _repository.GetDailyRevenueAsync(vendorId, from, to, cancellationToken);

        return new VendorDailyRevenueDTO
        {
            From = from,
            To = to,
            DailyRevenues = dailyData.Select(d => new VendorDailyRevenueItemDTO
            {
                Date = d.date,
                GrossRevenue = d.grossRevenue,
                NetRevenue = d.grossRevenue - d.commission,
                OrderCount = d.orderCount
            }).ToList(),
            TotalGrossRevenue = dailyData.Sum(d => d.grossRevenue),
            TotalNetRevenue = dailyData.Sum(d => d.grossRevenue - d.commission),
            TotalOrders = dailyData.Sum(d => d.orderCount)
        };
    }

    public async Task<VendorMonthlyRevenueDTO> GetMonthlyRevenueAsync(ulong vendorId, int year, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        if (year < 2020 || year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Năm không hợp lệ.", nameof(year));

        var monthlyData = await _repository.GetMonthlyRevenueAsync(vendorId, year, cancellationToken);

        return new VendorMonthlyRevenueDTO
        {
            Year = year,
            MonthlyRevenues = monthlyData.Select(m => new VendorMonthlyRevenueItemDTO
            {
                Month = m.month,
                MonthName = MonthNames[m.month - 1],
                GrossRevenue = m.grossRevenue,
                NetRevenue = m.grossRevenue - m.commission,
                OrderCount = m.orderCount
            }).ToList(),
            TotalGrossRevenue = monthlyData.Sum(m => m.grossRevenue),
            TotalNetRevenue = monthlyData.Sum(m => m.grossRevenue - m.commission),
            TotalOrders = monthlyData.Sum(m => m.orderCount)
        };
    }

    public async Task<VendorOrderStatisticsDTO> GetOrderStatisticsAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);
        ValidateDateRange(from, to);

        var statusCounts = await _repository.GetOrderCountsByStatusAsync(vendorId, from, to, cancellationToken);
        var avgDeliveryDays = await _repository.GetAverageDeliveryDaysAsync(vendorId, from, to, cancellationToken);

        var total = statusCounts.Values.Sum();
        var delivered = statusCounts.GetValueOrDefault("Delivered");
        var cancelled = statusCounts.GetValueOrDefault("Cancelled");
        var refunded = statusCounts.GetValueOrDefault("Refunded") + statusCounts.GetValueOrDefault("PartialRefund");

        return new VendorOrderStatisticsDTO
        {
            From = from,
            To = to,
            TotalOrders = total,
            OrdersByStatus = new VendorOrdersByStatusDTO
            {
                Pending = statusCounts.GetValueOrDefault("Pending"),
                Processing = statusCounts.GetValueOrDefault("Processing"),
                Paid = statusCounts.GetValueOrDefault("Paid"),
                Shipped = statusCounts.GetValueOrDefault("Shipped"),
                Delivered = delivered,
                Cancelled = cancelled,
                Refunded = statusCounts.GetValueOrDefault("Refunded"),
                PartialRefund = statusCounts.GetValueOrDefault("PartialRefund")
            },
            FulfillmentRate = total > 0 ? Math.Round((decimal)delivered / total * 100, 2) : 0,
            CancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 2) : 0,
            RefundRate = total > 0 ? Math.Round((decimal)refunded / total * 100, 2) : 0,
            AverageDeliveryDays = Math.Round(avgDeliveryDays, 2)
        };
    }

    public async Task<VendorProductStatisticsDTO> GetProductStatisticsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var (total, active, inactive, outOfStock, lowStock, totalStock, totalValue) = await _repository.GetProductStatisticsAsync(vendorId, cancellationToken);
        var categoryDist = await _repository.GetProductCategoryDistributionAsync(vendorId, cancellationToken);

        return new VendorProductStatisticsDTO
        {
            TotalProducts = total,
            ActiveProducts = active,
            InactiveProducts = inactive,
            OutOfStockProducts = outOfStock,
            LowStockProducts = lowStock,
            TotalStockQuantity = totalStock,
            TotalStockValue = totalValue,
            CategoryDistribution = categoryDist.Select(c => new VendorCategoryDistributionDTO
            {
                CategoryId = c.categoryId,
                CategoryName = c.categoryName,
                ProductCount = c.productCount,
                Percentage = active > 0 ? Math.Round((decimal)c.productCount / active * 100, 2) : 0
            }).ToList()
        };
    }

    public async Task<VendorBestSellingProductsDTO> GetBestSellingProductsAsync(ulong vendorId, DateOnly? from, DateOnly? to, int limit, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;
        limit = Math.Clamp(limit, 1, 20);

        ValidateDateRange(fromDate, toDate);

        var products = await _repository.GetBestSellingProductsAsync(vendorId, fromDate, toDate, limit, cancellationToken);

        return new VendorBestSellingProductsDTO
        {
            From = fromDate,
            To = toDate,
            Products = products.Select((p, index) => new VendorBestSellingProductItemDTO
            {
                Rank = index + 1,
                ProductId = p.product.Id,
                ProductCode = p.product.ProductCode,
                ProductName = p.product.ProductName,
                Slug = p.product.Slug,
                ImageUrl = p.imageUrl,
                UnitPrice = p.product.UnitPrice,
                SoldQuantity = p.soldQuantity,
                TotalRevenue = p.totalRevenue,
                StockQuantity = p.product.StockQuantity,
                RatingAverage = p.product.RatingAverage
            }).ToList()
        };
    }

    public async Task<VendorProductRatingsDTO> GetProductRatingsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var (avgRating, totalReviews) = await _repository.GetRatingInfoAsync(vendorId, cancellationToken);
        var (star5, star4, star3, star2, star1) = await _repository.GetRatingDistributionAsync(vendorId, cancellationToken);
        var topHighest = await _repository.GetTopRatedProductsAsync(vendorId, 3, false, cancellationToken);
        var topLowest = await _repository.GetTopRatedProductsAsync(vendorId, 3, true, cancellationToken);

        return new VendorProductRatingsDTO
        {
            AverageRating = avgRating,
            TotalReviews = totalReviews,
            RatingDistribution = new VendorRatingDistributionDTO
            {
                Star5 = star5,
                Star4 = star4,
                Star3 = star3,
                Star2 = star2,
                Star1 = star1
            },
            Top3Highest = topHighest.Select(p => new VendorProductRatingItemDTO
            {
                ProductId = p.product.Id,
                ProductCode = p.product.ProductCode,
                ProductName = p.product.ProductName,
                ImageUrl = p.imageUrl,
                RatingAverage = p.product.RatingAverage,
                ReviewCount = p.reviewCount
            }).ToList(),
            Top3Lowest = topLowest.Select(p => new VendorProductRatingItemDTO
            {
                ProductId = p.product.Id,
                ProductCode = p.product.ProductCode,
                ProductName = p.product.ProductName,
                ImageUrl = p.imageUrl,
                RatingAverage = p.product.RatingAverage,
                ReviewCount = p.reviewCount
            }).ToList()
        };
    }

    public async Task<VendorWalletStatisticsDTO> GetWalletStatisticsAsync(ulong vendorId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? today.AddDays(-30);
        var toDate = to ?? today;

        ValidateDateRange(fromDate, toDate);

        var (walletBalance, pendingCashout) = await _repository.GetWalletInfoAsync(vendorId, cancellationToken);
        var (totalTopup, totalCashout, topupCount, cashoutCount) = await _repository.GetTransactionSummaryAsync(vendorId, fromDate, toDate, cancellationToken);
        var (pendingAmount, pendingOrderCount) = await _repository.GetPendingCreditsAsync(vendorId, cancellationToken);
        var recentTransactions = await _repository.GetRecentTransactionsAsync(vendorId, 5, cancellationToken);

        return new VendorWalletStatisticsDTO
        {
            CurrentBalance = walletBalance,
            PendingCashout = pendingCashout,
            AvailableBalance = walletBalance - pendingCashout,
            From = fromDate,
            To = toDate,
            TransactionSummary = new VendorTransactionSummaryDTO
            {
                TotalTopup = totalTopup,
                TotalCashout = totalCashout,
                TopupCount = topupCount,
                CashoutCount = cashoutCount
            },
            PendingCredits = new VendorPendingCreditsDTO
            {
                Amount = pendingAmount,
                OrderCount = pendingOrderCount
            },
            RecentTransactions = recentTransactions.Select(t => new VendorRecentTransactionDTO
            {
                TransactionId = t.Id,
                TransactionType = t.TransactionType.ToString(),
                Amount = t.Amount,
                OrderId = t.OrderId,
                Status = t.Status.ToString(),
                Note = t.Note,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }

    public async Task<VendorPendingItemsDTO> GetPendingItemsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        await ValidateVendorAsync(vendorId, cancellationToken);

        var registrations = await _repository.GetPendingProductRegistrationsAsync(vendorId, cancellationToken);
        var updateRequests = await _repository.GetPendingProductUpdateRequestsAsync(vendorId, cancellationToken);
        var vendorCerts = await _repository.GetPendingVendorCertificatesAsync(vendorId, cancellationToken);
        var productCerts = await _repository.GetPendingProductCertificatesAsync(vendorId, cancellationToken);
        var cashouts = await _repository.GetPendingCashoutRequestsAsync(vendorId, cancellationToken);

        return new VendorPendingItemsDTO
        {
            ProductRegistrations = new VendorPendingProductRegistrationsDTO
            {
                Count = registrations.Count,
                Items = registrations.Select(r => new VendorPendingProductRegistrationItemDTO
                {
                    Id = r.Id,
                    ProposedProductName = r.ProposedProductName,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt
                }).ToList()
            },
            ProductUpdateRequests = new VendorPendingProductUpdatesDTO
            {
                Count = updateRequests.Count,
                Items = updateRequests.Select(r => new VendorPendingProductUpdateItemDTO
                {
                    Id = r.request.Id,
                    ProductId = r.request.ProductId,
                    ProductName = r.productName,
                    Status = r.request.Status.ToString(),
                    CreatedAt = r.request.CreatedAt
                }).ToList()
            },
            VendorCertificates = new VendorPendingCertificatesDTO
            {
                Count = vendorCerts.Count,
                Items = vendorCerts.Select(c => new VendorPendingCertificateItemDTO
                {
                    Id = c.Id,
                    CertificationName = c.CertificationName,
                    Status = c.Status.ToString(),
                    UploadedAt = c.UploadedAt
                }).ToList()
            },
            ProductCertificates = new VendorPendingProductCertificatesDTO
            {
                Count = productCerts.Count,
                Items = productCerts.Select(c => new VendorPendingProductCertificateItemDTO
                {
                    Id = c.cert.Id,
                    ProductId = c.productId,
                    CertificationName = c.cert.CertificationName,
                    Status = c.cert.Status.ToString(),
                    UploadedAt = c.cert.UploadedAt
                }).ToList()
            },
            CashoutRequests = new VendorPendingCashoutDTO
            {
                Count = cashouts.Count,
                Items = cashouts.Select(c => new VendorPendingCashoutItemDTO
                {
                    TransactionId = c.Id,
                    Amount = c.Amount,
                    BankAccountNumber = c.BankAccount != null ? MaskAccountNumber(c.BankAccount.AccountNumber) : "N/A",
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                }).ToList()
            }
        };
    }

    #region Private Helpers

    private async Task ValidateVendorAsync(ulong vendorId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetVerifiedAndActiveUserByIdAsync(vendorId, cancellationToken);
        if (user.Role != UserRole.Vendor)
            throw new UnauthorizedAccessException("Chỉ nhà cung cấp mới có thể truy cập thông tin này.");
    }

    private static void ValidateDateRange(DateOnly from, DateOnly to, int maxDays = 365)
    {
        if (from > to)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");

        if ((to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days > maxDays)
            throw new ArgumentException($"Khoảng thời gian không được vượt quá {maxDays} ngày.");
    }

    private static decimal CalculateGrowthPercent(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / previous * 100, 2);
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
            return accountNumber;
        return "****" + accountNumber[^4..];
    }

    #endregion
}

