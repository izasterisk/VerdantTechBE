using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class VendorDashboardRepository : IVendorDashboardRepository
{
    private readonly VerdantTechDbContext _dbContext;
    private const int LowStockThreshold = 10;

    public VendorDashboardRepository(VerdantTechDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region Overview

    public async Task<(decimal walletBalance, decimal pendingCashout)> GetWalletInfoAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.VendorId == vendorId, cancellationToken);

        var pendingCashout = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == vendorId && t.TransactionType == TransactionType.WalletCashout && t.Status == TransactionStatus.Pending)
            .SumAsync(t => t.Amount, cancellationToken);

        return (wallet?.Balance ?? 0, pendingCashout);
    }

    public async Task<(decimal thisMonth, decimal lastMonth)> GetMonthlyRevenueComparisonAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var thisMonth = await GetVendorGrossRevenueAsync(vendorId, thisMonthStart, now, cancellationToken);
        var lastMonth = await GetVendorGrossRevenueAsync(vendorId, lastMonthStart, thisMonthStart, cancellationToken);

        return (thisMonth, lastMonth);
    }

    public async Task<(int thisMonth, int lastMonth)> GetMonthlyOrderCountComparisonAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var thisMonth = await GetVendorOrderCountAsync(vendorId, thisMonthStart, now, cancellationToken);
        var lastMonth = await GetVendorOrderCountAsync(vendorId, lastMonthStart, thisMonthStart, cancellationToken);

        return (thisMonth, lastMonth);
    }

    public async Task<(int active, int outOfStock, int pendingRegistrations, int pendingUpdates)> GetProductCountsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var active = await _dbContext.Products
            .AsNoTracking()
            .CountAsync(p => p.VendorId == vendorId && p.IsActive, cancellationToken);

        var outOfStock = await _dbContext.Products
            .AsNoTracking()
            .CountAsync(p => p.VendorId == vendorId && p.IsActive && p.StockQuantity == 0, cancellationToken);

        var pendingRegistrations = await _dbContext.ProductRegistrations
            .AsNoTracking()
            .CountAsync(p => p.VendorId == vendorId && p.Status == ProductRegistrationStatus.Pending, cancellationToken);

        var pendingUpdates = await _dbContext.ProductUpdateRequests
            .AsNoTracking()
            .Include(p => p.ProductSnapshot)
            .CountAsync(p => p.ProductSnapshot.VendorId == vendorId && p.Status == ProductRegistrationStatus.Pending, cancellationToken);

        return (active, outOfStock, pendingRegistrations, pendingUpdates);
    }

    public async Task<(decimal averageRating, int totalReviews)> GetRatingInfoAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId && p.IsActive && p.RatingAverage > 0)
            .Select(p => p.RatingAverage)
            .ToListAsync(cancellationToken);

        var totalReviews = await _dbContext.ProductReviews
            .AsNoTracking()
            .CountAsync(r => r.Product.VendorId == vendorId, cancellationToken);

        var averageRating = products.Count > 0 ? products.Average() : 0;

        return (averageRating, totalReviews);
    }

    #endregion

    #region Revenue

    public async Task<(decimal grossRevenue, decimal commission, int orderCount)> GetRevenueByTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var data = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => 1)
            .Select(g => new
            {
                GrossRevenue = g.Sum(od => od.Subtotal),
                Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100),
                OrderCount = g.Select(od => od.OrderId).Distinct().Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return (data?.GrossRevenue ?? 0, data?.Commission ?? 0, data?.OrderCount ?? 0);
    }

    public async Task<List<(DateOnly date, decimal grossRevenue, decimal commission, int orderCount)>> GetDailyRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var data = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => od.Order.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                GrossRevenue = g.Sum(od => od.Subtotal),
                Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100),
                OrderCount = g.Select(od => od.OrderId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        // Fill missing dates with zeros
        var result = new List<(DateOnly date, decimal grossRevenue, decimal commission, int orderCount)>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var item = data.FirstOrDefault(d => DateOnly.FromDateTime(d.Date) == date);
            result.Add((date, item?.GrossRevenue ?? 0, item?.Commission ?? 0, item?.OrderCount ?? 0));
        }

        return result;
    }

    public async Task<List<(int month, decimal grossRevenue, decimal commission, int orderCount)>> GetMonthlyRevenueAsync(ulong vendorId, int year, CancellationToken cancellationToken = default)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var data = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= yearStart && od.Order.CreatedAt < yearEnd)
            .GroupBy(od => od.Order.CreatedAt.Month)
            .Select(g => new
            {
                Month = g.Key,
                GrossRevenue = g.Sum(od => od.Subtotal),
                Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100),
                OrderCount = g.Select(od => od.OrderId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        // Fill all 12 months
        var result = new List<(int month, decimal grossRevenue, decimal commission, int orderCount)>();
        for (int month = 1; month <= 12; month++)
        {
            var item = data.FirstOrDefault(d => d.Month == month);
            result.Add((month, item?.GrossRevenue ?? 0, item?.Commission ?? 0, item?.OrderCount ?? 0));
        }

        return result;
    }

    #endregion

    #region Orders

    public async Task<Dictionary<string, int>> GetOrderCountsByStatusAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var orderIds = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .Select(od => od.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var statusCounts = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statusCounts.ToDictionary(x => x.Status.ToString(), x => x.Count);
    }

    public async Task<decimal> GetAverageDeliveryDaysAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var orderIds = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Select(od => od.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var deliveredOrders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .Where(o => o.Status == OrderStatus.Delivered && o.ShippedAt != null && o.DeliveredAt != null)
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .Select(o => new { o.ShippedAt, o.DeliveredAt })
            .ToListAsync(cancellationToken);

        if (deliveredOrders.Count == 0) return 0;

        var totalDays = deliveredOrders.Sum(o => (o.DeliveredAt!.Value - o.ShippedAt!.Value).TotalDays);
        return (decimal)(totalDays / deliveredOrders.Count);
    }

    #endregion

    #region Products

    public async Task<(int total, int active, int inactive, int outOfStock, int lowStock, int totalStock, decimal totalValue)> GetProductStatisticsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId)
            .Select(p => new { p.IsActive, p.StockQuantity, p.UnitPrice })
            .ToListAsync(cancellationToken);

        var total = products.Count;
        var active = products.Count(p => p.IsActive);
        var inactive = products.Count(p => !p.IsActive);
        var outOfStock = products.Count(p => p.IsActive && p.StockQuantity == 0);
        var lowStock = products.Count(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity < LowStockThreshold);
        var totalStock = products.Sum(p => p.StockQuantity);
        var totalValue = products.Sum(p => p.StockQuantity * p.UnitPrice);

        return (total, active, inactive, outOfStock, lowStock, totalStock, totalValue);
    }

    public async Task<List<(ulong categoryId, string categoryName, int productCount)>> GetProductCategoryDistributionAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId && p.IsActive)
            .GroupBy(p => new { p.CategoryId, p.Category.Name })
            .Select(g => new { CategoryId = g.Key.CategoryId, CategoryName = g.Key.Name, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => ValueTuple.Create(x.CategoryId, x.CategoryName, x.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(Product product, string? imageUrl, int soldQuantity, decimal totalRevenue)>> GetBestSellingProductsAsync(ulong vendorId, DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var topProducts = await _dbContext.OrderDetails
            .AsNoTracking()
            .Include(od => od.Product)
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                SoldQuantity = g.Sum(od => od.Quantity),
                TotalRevenue = g.Sum(od => od.Subtotal)
            })
            .OrderByDescending(x => x.SoldQuantity)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var productIds = topProducts.Select(x => x.Product.Id).ToList();
        var images = await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.Products && productIds.Contains(ml.OwnerId))
            .GroupBy(ml => ml.OwnerId)
            .Select(g => new { ProductId = g.Key, ImageUrl = g.OrderBy(ml => ml.SortOrder).First().ImageUrl })
            .ToDictionaryAsync(x => x.ProductId, x => x.ImageUrl, cancellationToken);

        return topProducts.Select(tp => (
            tp.Product,
            images.TryGetValue(tp.Product.Id, out var img) ? img : null,
            tp.SoldQuantity,
            tp.TotalRevenue
        )).ToList();
    }

    public async Task<(int star5, int star4, int star3, int star2, int star1)> GetRatingDistributionAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var distribution = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.Product.VendorId == vendorId)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, cancellationToken);

        return (
            distribution.GetValueOrDefault(5),
            distribution.GetValueOrDefault(4),
            distribution.GetValueOrDefault(3),
            distribution.GetValueOrDefault(2),
            distribution.GetValueOrDefault(1)
        );
    }

    public async Task<List<(Product product, string? imageUrl, int reviewCount)>> GetTopRatedProductsAsync(ulong vendorId, int limit, bool ascending, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId && p.IsActive && p.RatingAverage > 0);

        query = ascending
            ? query.OrderBy(p => p.RatingAverage)
            : query.OrderByDescending(p => p.RatingAverage);

        var products = await query.Take(limit).ToListAsync(cancellationToken);

        var productIds = products.Select(p => p.Id).ToList();

        var images = await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.Products && productIds.Contains(ml.OwnerId))
            .GroupBy(ml => ml.OwnerId)
            .Select(g => new { ProductId = g.Key, ImageUrl = g.OrderBy(ml => ml.SortOrder).First().ImageUrl })
            .ToDictionaryAsync(x => x.ProductId, x => x.ImageUrl, cancellationToken);

        var reviewCounts = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count, cancellationToken);

        return products.Select(p => (
            p,
            images.TryGetValue(p.Id, out var img) ? img : null,
            reviewCounts.GetValueOrDefault(p.Id)
        )).ToList();
    }

    #endregion

    #region Wallet

    public async Task<(decimal totalTopup, decimal totalCashout, int topupCount, int cashoutCount)> GetTransactionSummaryAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var transactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == vendorId && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .Where(t => t.TransactionType == TransactionType.WalletTopup || t.TransactionType == TransactionType.WalletCashout)
            .GroupBy(t => t.TransactionType)
            .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var topup = transactions.FirstOrDefault(t => t.Type == TransactionType.WalletTopup);
        var cashout = transactions.FirstOrDefault(t => t.Type == TransactionType.WalletCashout);

        return (topup?.Total ?? 0, cashout?.Total ?? 0, topup?.Count ?? 0, cashout?.Count ?? 0);
    }

    public async Task<(decimal amount, int orderCount)> GetPendingCreditsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var pendingOrders = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.IsWalletCredited == false)
            .Where(od => od.Order.DeliveredAt == null || od.Order.DeliveredAt >= sevenDaysAgo)
            .GroupBy(od => 1)
            .Select(g => new
            {
                Amount = g.Sum(od => od.Subtotal * (100 - od.Product.CommissionRate) / 100),
                OrderCount = g.Select(od => od.OrderId).Distinct().Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return (pendingOrders?.Amount ?? 0, pendingOrders?.OrderCount ?? 0);
    }

    public async Task<List<Transaction>> GetRecentTransactionsAsync(ulong vendorId, int limit, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == vendorId)
            .Where(t => t.TransactionType == TransactionType.WalletTopup || t.TransactionType == TransactionType.WalletCashout)
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Pending Items

    public async Task<List<ProductRegistration>> GetPendingProductRegistrationsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductRegistrations
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId && p.Status == ProductRegistrationStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(ProductUpdateRequest request, string productName)>> GetPendingProductUpdateRequestsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var requests = await _dbContext.ProductUpdateRequests
            .AsNoTracking()
            .Include(p => p.ProductSnapshot)
            .Include(p => p.Product)
            .Where(p => p.ProductSnapshot.VendorId == vendorId && p.Status == ProductRegistrationStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => (r, r.Product.ProductName)).ToList();
    }

    public async Task<List<VendorCertificate>> GetPendingVendorCertificatesAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VendorCertificates
            .AsNoTracking()
            .Where(c => c.VendorId == vendorId && c.Status == VendorCertificateStatus.Pending)
            .OrderByDescending(c => c.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<(ProductCertificate cert, ulong productId)>> GetPendingProductCertificatesAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var certs = await _dbContext.ProductCertificates
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.Product != null && c.Product.VendorId == vendorId && c.Status == ProductCertificateStatus.Pending)
            .OrderByDescending(c => c.UploadedAt)
            .ToListAsync(cancellationToken);

        return certs.Select(c => (c, c.ProductId ?? 0)).ToList();
    }

    public async Task<List<Transaction>> GetPendingCashoutRequestsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.BankAccount)
            .Where(t => t.UserId == vendorId && t.TransactionType == TransactionType.WalletCashout && t.Status == TransactionStatus.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Transaction Export

    public async Task<List<Transaction>> GetAllTransactionsInTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == vendorId)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .Include(t => t.User)
            .Include(t => t.ProcessedByNavigation)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Transaction> transactions, int totalCount)> GetTransactionsWithPagingAsync(ulong vendorId, DateOnly from, DateOnly to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == vendorId)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Include(t => t.User)
            .Include(t => t.ProcessedByNavigation)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    #endregion

    #region Private Helpers

    private async Task<decimal> GetVendorGrossRevenueAsync(ulong vendorId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= from && od.Order.CreatedAt < to)
            .SumAsync(od => od.Subtotal, cancellationToken);
    }

    private async Task<int> GetVendorOrderCountAsync(ulong vendorId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Product.VendorId == vendorId)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= from && od.Order.CreatedAt < to)
            .Select(od => od.OrderId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    #endregion
}

