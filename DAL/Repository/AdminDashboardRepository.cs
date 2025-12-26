using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly VerdantTechDbContext _dbContext;
    private const int LowStockThreshold = 10;

    public AdminDashboardRepository(VerdantTechDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    #region Overview

    public async Task<(decimal today, decimal thisWeek, decimal thisMonth, decimal lastMonth)> GetRevenueOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var today = await GetSystemRevenueAsync(todayStart, now, cancellationToken);
        var thisWeek = await GetSystemRevenueAsync(weekStart, now, cancellationToken);
        var thisMonth = await GetSystemRevenueAsync(monthStart, now, cancellationToken);
        var lastMonth = await GetSystemRevenueAsync(lastMonthStart, monthStart, cancellationToken);

        return (today, thisWeek, thisMonth, lastMonth);
    }

    public async Task<(decimal thisMonth, decimal lastMonth)> GetCommissionOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var thisMonth = await GetSystemCommissionAsync(monthStart, now, cancellationToken);
        var lastMonth = await GetSystemCommissionAsync(lastMonthStart, monthStart, cancellationToken);

        return (thisMonth, lastMonth);
    }

    public async Task<(int today, int thisWeek, int thisMonth, int pendingShipment, int inTransit)> GetOrdersOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var today = await _dbContext.Orders.AsNoTracking().CountAsync(o => o.CreatedAt >= todayStart, cancellationToken);
        var thisWeek = await _dbContext.Orders.AsNoTracking().CountAsync(o => o.CreatedAt >= weekStart, cancellationToken);
        var thisMonth = await _dbContext.Orders.AsNoTracking().CountAsync(o => o.CreatedAt >= monthStart, cancellationToken);
        var pendingShipment = await _dbContext.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Paid, cancellationToken);
        var inTransit = await _dbContext.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Shipped, cancellationToken);

        return (today, thisWeek, thisMonth, pendingShipment, inTransit);
    }

    public async Task<(int totalCustomers, int totalVendors, int newCustomersThisMonth, int newVendorsThisMonth)> GetUsersOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalCustomers = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Customer && u.Status != UserStatus.Deleted, cancellationToken);
        var totalVendors = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status != UserStatus.Deleted, cancellationToken);
        var newCustomersThisMonth = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Customer && u.CreatedAt >= monthStart, cancellationToken);
        var newVendorsThisMonth = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.CreatedAt >= monthStart, cancellationToken);

        return (totalCustomers, totalVendors, newCustomersThisMonth, newVendorsThisMonth);
    }

    public async Task<(int active, int inactive, int outOfStock)> GetProductsOverviewAsync(CancellationToken cancellationToken = default)
    {
        var active = await _dbContext.Products.AsNoTracking().CountAsync(p => p.IsActive, cancellationToken);
        var inactive = await _dbContext.Products.AsNoTracking().CountAsync(p => !p.IsActive, cancellationToken);
        var outOfStock = await _dbContext.Products.AsNoTracking().CountAsync(p => p.IsActive && p.StockQuantity == 0, cancellationToken);

        return (active, inactive, outOfStock);
    }

    public async Task<(int vendorProfiles, int productRegistrations, int vendorCertificates, int productCertificates, 
        int productUpdateRequests, int supportRequests, int refundRequests, int cashoutRequests)> GetPendingQueuesOverviewAsync(CancellationToken cancellationToken = default)
    {
        var vendorProfiles = await _dbContext.VendorProfiles.AsNoTracking().CountAsync(v => v.VerifiedAt == null && v.VerifiedBy == null, cancellationToken);
        var productRegistrations = await _dbContext.ProductRegistrations.AsNoTracking().CountAsync(p => p.Status == ProductRegistrationStatus.Pending, cancellationToken);
        var vendorCertificates = await _dbContext.VendorCertificates.AsNoTracking().CountAsync(c => c.Status == VendorCertificateStatus.Pending, cancellationToken);
        var productCertificates = await _dbContext.ProductCertificates.AsNoTracking().CountAsync(c => c.Status == ProductCertificateStatus.Pending, cancellationToken);
        var productUpdateRequests = await _dbContext.ProductUpdateRequests.AsNoTracking().CountAsync(p => p.Status == ProductRegistrationStatus.Pending, cancellationToken);
        var supportRequests = await _dbContext.Requests.AsNoTracking().CountAsync(r => r.RequestType == RequestType.SupportRequest && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InReview), cancellationToken);
        var refundRequests = await _dbContext.Requests.AsNoTracking().CountAsync(r => r.RequestType == RequestType.RefundRequest && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InReview), cancellationToken);
        var cashoutRequests = await _dbContext.Transactions.AsNoTracking().CountAsync(t => t.TransactionType == TransactionType.WalletCashout && t.Status == TransactionStatus.Pending, cancellationToken);

        return (vendorProfiles, productRegistrations, vendorCertificates, productCertificates, productUpdateRequests, supportRequests, refundRequests, cashoutRequests);
    }

    #endregion

    #region Revenue

    public async Task<(decimal totalRevenue, decimal totalCommission, int totalOrders, int totalTransactions)> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var totalRevenue = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .SumAsync(t => t.Amount, cancellationToken);

        var totalCommission = await GetSystemCommissionAsync(fromDateTime, toDateTime, cancellationToken);

        var totalOrders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            .CountAsync(cancellationToken);

        var totalTransactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .CountAsync(cancellationToken);

        return (totalRevenue, totalCommission, totalOrders, totalTransactions);
    }

    public async Task<(int bankingCount, decimal bankingAmount, int codCount, decimal codAmount)> GetPaymentMethodBreakdownAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var breakdown = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
            .GroupBy(o => o.OrderPaymentMethod)
            .Select(g => new { Method = g.Key, Count = g.Count(), Amount = g.Sum(o => o.TotalAmount) })
            .ToListAsync(cancellationToken);

        var banking = breakdown.FirstOrDefault(b => b.Method == OrderPaymentMethod.Banking);
        var cod = breakdown.FirstOrDefault(b => b.Method == OrderPaymentMethod.COD);

        return (banking?.Count ?? 0, banking?.Amount ?? 0, cod?.Count ?? 0, cod?.Amount ?? 0);
    }

    public async Task<List<(DateOnly date, decimal revenue, decimal commission, int orderCount, int transactionCount)>> GetDailyRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var revenueData = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(t => t.Amount), TransactionCount = g.Count() })
            .ToListAsync(cancellationToken);

        var commissionData = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => od.Order.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100), OrderCount = g.Select(od => od.OrderId).Distinct().Count() })
            .ToListAsync(cancellationToken);

        var result = new List<(DateOnly date, decimal revenue, decimal commission, int orderCount, int transactionCount)>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var rev = revenueData.FirstOrDefault(r => DateOnly.FromDateTime(r.Date) == date);
            var comm = commissionData.FirstOrDefault(c => DateOnly.FromDateTime(c.Date) == date);
            result.Add((date, rev?.Revenue ?? 0, comm?.Commission ?? 0, comm?.OrderCount ?? 0, rev?.TransactionCount ?? 0));
        }

        return result;
    }

    public async Task<List<(int month, decimal revenue, decimal commission, int orderCount)>> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var revenueData = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= yearStart && t.CreatedAt < yearEnd)
            .GroupBy(t => t.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Revenue = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var commissionData = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= yearStart && od.Order.CreatedAt < yearEnd)
            .GroupBy(od => od.Order.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100), OrderCount = g.Select(od => od.OrderId).Distinct().Count() })
            .ToListAsync(cancellationToken);

        var result = new List<(int month, decimal revenue, decimal commission, int orderCount)>();
        for (int month = 1; month <= 12; month++)
        {
            var rev = revenueData.FirstOrDefault(r => r.Month == month);
            var comm = commissionData.FirstOrDefault(c => c.Month == month);
            result.Add((month, rev?.Revenue ?? 0, comm?.Commission ?? 0, comm?.OrderCount ?? 0));
        }

        return result;
    }

    #endregion

    #region Orders

    public async Task<Dictionary<string, int>> GetOrderCountsByStatusAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var statusCounts = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statusCounts.ToDictionary(x => x.Status.ToString(), x => x.Count);
    }

    public async Task<(int bankingCount, int codCount)> GetOrderCountsByPaymentMethodAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var counts = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .GroupBy(o => o.OrderPaymentMethod)
            .Select(g => new { Method = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return (
            counts.FirstOrDefault(c => c.Method == OrderPaymentMethod.Banking)?.Count ?? 0,
            counts.FirstOrDefault(c => c.Method == OrderPaymentMethod.COD)?.Count ?? 0
        );
    }

    public async Task<List<(int courierId, int orderCount)>> GetOrderCountsByCourierAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var counts = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .GroupBy(o => o.CourierId)
            .Select(g => new { CourierId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return counts.Select(c => (c.CourierId, c.Count)).ToList();
    }

    public async Task<decimal> GetAverageDeliveryDaysAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var deliveredOrders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && o.ShippedAt != null && o.DeliveredAt != null)
            .Where(o => o.CreatedAt >= fromDateTime && o.CreatedAt < toDateTime)
            .Select(o => new { o.ShippedAt, o.DeliveredAt })
            .ToListAsync(cancellationToken);

        if (deliveredOrders.Count == 0) return 0;

        var totalDays = deliveredOrders.Sum(o => (o.DeliveredAt!.Value - o.ShippedAt!.Value).TotalDays);
        return (decimal)(totalDays / deliveredOrders.Count);
    }

    #endregion

    #region Users

    public async Task<(int total, int active, int inactive, int newThisPeriod)> GetCustomerStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);

        var total = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Customer && u.Status != UserStatus.Deleted, cancellationToken);
        
        var activeCustomerIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= ninetyDaysAgo)
            .Select(o => o.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var active = activeCustomerIds.Count;
        
        var inactive = total - active;
        var newThisPeriod = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Customer && u.CreatedAt >= fromDateTime && u.CreatedAt < toDateTime, cancellationToken);

        return (total, active, inactive, newThisPeriod);
    }

    public async Task<(int total, int verified, int pending, int newThisPeriod)> GetVendorStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var total = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status != UserStatus.Deleted, cancellationToken);
        var verified = await _dbContext.VendorProfiles.AsNoTracking().CountAsync(v => v.VerifiedAt != null, cancellationToken);
        var pending = await _dbContext.VendorProfiles.AsNoTracking().CountAsync(v => v.VerifiedAt == null && v.VerifiedBy == null, cancellationToken);
        var newThisPeriod = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.CreatedAt >= fromDateTime && u.CreatedAt < toDateTime, cancellationToken);

        return (total, verified, pending, newThisPeriod);
    }

    public async Task<(int total, int active)> GetStaffStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Users.AsNoTracking().CountAsync(u => (u.Role == UserRole.Staff || u.Role == UserRole.Admin) && u.Status != UserStatus.Deleted, cancellationToken);
        var active = await _dbContext.Users.AsNoTracking().CountAsync(u => (u.Role == UserRole.Staff || u.Role == UserRole.Admin) && u.Status == UserStatus.Active, cancellationToken);

        return (total, active);
    }

    public async Task<List<(DateOnly date, int customers, int vendors)>> GetRegistrationTrendAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var data = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= fromDateTime && u.CreatedAt < toDateTime)
            .Where(u => u.Role == UserRole.Customer || u.Role == UserRole.Vendor)
            .GroupBy(u => new { Date = u.CreatedAt.Date, u.Role })
            .Select(g => new { Date = g.Key.Date, Role = g.Key.Role, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new List<(DateOnly date, int customers, int vendors)>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var customers = data.Where(d => DateOnly.FromDateTime(d.Date) == date && d.Role == UserRole.Customer).Sum(d => d.Count);
            var vendors = data.Where(d => DateOnly.FromDateTime(d.Date) == date && d.Role == UserRole.Vendor).Sum(d => d.Count);
            result.Add((date, customers, vendors));
        }

        return result;
    }

    #endregion

    #region Products

    public async Task<(int total, int active, int inactive, int outOfStock, int lowStock)> GetProductStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Select(p => new { p.IsActive, p.StockQuantity })
            .ToListAsync(cancellationToken);

        return (
            products.Count,
            products.Count(p => p.IsActive),
            products.Count(p => !p.IsActive),
            products.Count(p => p.IsActive && p.StockQuantity == 0),
            products.Count(p => p.IsActive && p.StockQuantity > 0 && p.StockQuantity < LowStockThreshold)
        );
    }

    public async Task<List<(ulong categoryId, string categoryName, int productCount, int activeCount, int outOfStockCount)>> GetCategoryDistributionAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .GroupBy(p => new { p.CategoryId, p.Category.Name })
            .Select(g => ValueTuple.Create(
                g.Key.CategoryId,
                g.Key.Name,
                g.Count(),
                g.Count(p => p.IsActive),
                g.Count(p => p.IsActive && p.StockQuantity == 0)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<(int vendorsWithProducts, decimal avgProductsPerVendor, int topVendorProductCount)> GetVendorProductDistributionAsync(CancellationToken cancellationToken = default)
    {
        var vendorProductCounts = await _dbContext.Products
            .AsNoTracking()
            .GroupBy(p => p.VendorId)
            .Select(g => g.Count())
            .ToListAsync(cancellationToken);

        if (vendorProductCounts.Count == 0)
            return (0, 0, 0);

        return (
            vendorProductCounts.Count,
            (decimal)vendorProductCounts.Average(),
            vendorProductCounts.Max()
        );
    }

    public async Task<List<(Product product, string vendorName, string? imageUrl, int soldQuantity, decimal totalRevenue, decimal commission)>> GetBestSellingProductsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var topProducts = await _dbContext.OrderDetails
            .AsNoTracking()
            .Include(od => od.Product)
                .ThenInclude(p => p.Vendor)
                    .ThenInclude(v => v.VendorProfile)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                SoldQuantity = g.Sum(od => od.Quantity),
                TotalRevenue = g.Sum(od => od.Subtotal),
                Commission = g.Sum(od => od.Subtotal * g.First().Product.CommissionRate / 100)
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
            tp.Product.Vendor?.VendorProfile?.CompanyName ?? "N/A",
            images.TryGetValue(tp.Product.Id, out var img) ? img : null,
            tp.SoldQuantity,
            tp.TotalRevenue,
            tp.Commission
        )).ToList();
    }

    #endregion

    #region Vendors

    public async Task<(int total, int verified, int pending, int active, int inactive, int suspended)> GetVendorCountsAsync(CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status != UserStatus.Deleted, cancellationToken);
        var verified = await _dbContext.VendorProfiles.AsNoTracking().CountAsync(v => v.VerifiedAt != null, cancellationToken);
        var pending = await _dbContext.VendorProfiles.AsNoTracking().CountAsync(v => v.VerifiedAt == null && v.VerifiedBy == null, cancellationToken);
        var active = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status == UserStatus.Active, cancellationToken);
        var inactive = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status == UserStatus.Inactive, cancellationToken);
        var suspended = await _dbContext.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.Vendor && u.Status == UserStatus.Suspended, cancellationToken);

        return (total, verified, pending, active, inactive, suspended);
    }

    public async Task<decimal> GetTotalVendorRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .SumAsync(od => od.Subtotal * (100 - od.Product.CommissionRate) / 100, cancellationToken);
    }

    public async Task<List<(VendorProfile vendor, decimal grossRevenue, decimal commission, int orderCount, int productCount, decimal avgRating, decimal walletBalance)>> GetTopVendorsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var vendorRevenues = await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .GroupBy(od => od.Product.VendorId)
            .Select(g => new
            {
                VendorId = g.Key,
                GrossRevenue = g.Sum(od => od.Subtotal),
                Commission = g.Sum(od => od.Subtotal * od.Product.CommissionRate / 100),
                OrderCount = g.Select(od => od.OrderId).Distinct().Count()
            })
            .OrderByDescending(x => x.GrossRevenue)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var vendorIds = vendorRevenues.Select(v => v.VendorId).ToList();

        var vendors = await _dbContext.VendorProfiles
            .AsNoTracking()
            .Where(v => vendorIds.Contains(v.UserId))
            .ToDictionaryAsync(v => v.UserId, cancellationToken);

        var productCounts = await _dbContext.Products
            .AsNoTracking()
            .Where(p => vendorIds.Contains(p.VendorId) && p.IsActive)
            .GroupBy(p => p.VendorId)
            .Select(g => new { VendorId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VendorId, x => x.Count, cancellationToken);

        var avgRatings = await _dbContext.Products
            .AsNoTracking()
            .Where(p => vendorIds.Contains(p.VendorId) && p.RatingAverage > 0)
            .GroupBy(p => p.VendorId)
            .Select(g => new { VendorId = g.Key, AvgRating = g.Average(p => p.RatingAverage) })
            .ToDictionaryAsync(x => x.VendorId, x => x.AvgRating, cancellationToken);

        var walletBalances = await _dbContext.Wallets
            .AsNoTracking()
            .Where(w => vendorIds.Contains(w.VendorId))
            .ToDictionaryAsync(w => w.VendorId, w => w.Balance, cancellationToken);

        return vendorRevenues.Select(vr => (
            vendors.TryGetValue(vr.VendorId, out var vendor) ? vendor : null!,
            vr.GrossRevenue,
            vr.Commission,
            vr.OrderCount,
            productCounts.GetValueOrDefault(vr.VendorId),
            avgRatings.GetValueOrDefault(vr.VendorId),
            walletBalances.GetValueOrDefault(vr.VendorId)
        )).Where(x => x.Item1 != null).ToList();
    }

    #endregion

    #region Transactions
    
    public async Task<List<Transaction>> GetAllTransactionsInTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .Include(t => t.User)
            .Include(t => t.ProcessedByNavigation)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Transaction> transactions, int totalCount)> GetTransactionsWithPagingAsync(DateOnly from, DateOnly to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = _dbContext.Transactions
            .AsNoTracking()
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

    public async Task<(decimal totalInflow, decimal totalOutflow)> GetTransactionFlowAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var inflow = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .SumAsync(t => t.Amount, cancellationToken);

        var outflow = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => (t.TransactionType == TransactionType.WalletCashout || t.TransactionType == TransactionType.Refund) && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .SumAsync(t => t.Amount, cancellationToken);

        return (inflow, outflow);
    }

    public async Task<(int count, decimal completedAmount, decimal pendingAmount, int failedCount)> GetPaymentInStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var stats = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Amount = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var completed = stats.FirstOrDefault(s => s.Status == TransactionStatus.Completed);
        var pending = stats.FirstOrDefault(s => s.Status == TransactionStatus.Pending);
        var failed = stats.FirstOrDefault(s => s.Status == TransactionStatus.Failed);

        return (
            stats.Sum(s => s.Count),
            completed?.Amount ?? 0,
            pending?.Amount ?? 0,
            failed?.Count ?? 0
        );
    }

    public async Task<(int count, decimal completedAmount)> GetWalletTopupStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var stats = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.WalletTopup && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(t => t.Amount) })
            .FirstOrDefaultAsync(cancellationToken);

        return (stats?.Count ?? 0, stats?.Amount ?? 0);
    }

    public async Task<(int count, decimal completedAmount, int pendingCount, decimal pendingAmount)> GetWalletCashoutStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var stats = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.WalletCashout)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Amount = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var completed = stats.FirstOrDefault(s => s.Status == TransactionStatus.Completed);
        var pending = stats.FirstOrDefault(s => s.Status == TransactionStatus.Pending);

        return (
            stats.Sum(s => s.Count),
            completed?.Amount ?? 0,
            pending?.Count ?? 0,
            pending?.Amount ?? 0
        );
    }

    public async Task<(int count, decimal completedAmount, int pendingCount, decimal pendingAmount)> GetRefundStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var stats = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.Refund)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Amount = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var completed = stats.FirstOrDefault(s => s.Status == TransactionStatus.Completed);
        var pending = stats.FirstOrDefault(s => s.Status == TransactionStatus.Pending);

        return (
            stats.Sum(s => s.Count),
            completed?.Amount ?? 0,
            pending?.Count ?? 0,
            pending?.Amount ?? 0
        );
    }

    public async Task<List<(DateOnly date, decimal inflow, decimal outflow, int count)>> GetDailyTransactionTrendAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var data = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= fromDateTime && t.CreatedAt < toDateTime)
            .GroupBy(t => new { Date = t.CreatedAt.Date, t.TransactionType })
            .Select(g => new { Date = g.Key.Date, Type = g.Key.TransactionType, Amount = g.Sum(t => t.Amount), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new List<(DateOnly date, decimal inflow, decimal outflow, int count)>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var dateData = data.Where(d => DateOnly.FromDateTime(d.Date) == date).ToList();
            var inflow = dateData.Where(d => d.Type == TransactionType.PaymentIn).Sum(d => d.Amount);
            var outflow = dateData.Where(d => d.Type == TransactionType.WalletCashout || d.Type == TransactionType.Refund).Sum(d => d.Amount);
            var count = dateData.Sum(d => d.Count);
            result.Add((date, inflow, outflow, count));
        }

        return result;
    }

    #endregion

    #region Queue Statistics

    public async Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetVendorProfileQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.VendorProfiles
            .AsNoTracking()
            .Where(v => v.VerifiedAt == null && v.VerifiedBy == null)
            .Select(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return CalculateQueueStats(pending);
    }

    public async Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays, List<(ulong vendorId, string vendorName, int count)> byVendor)> GetProductRegistrationQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.ProductRegistrations
            .AsNoTracking()
            .Include(p => p.Vendor)
                .ThenInclude(v => v.VendorProfile)
            .Where(p => p.Status == ProductRegistrationStatus.Pending)
            .ToListAsync(cancellationToken);

        var stats = CalculateQueueStats(pending.Select(p => p.CreatedAt).ToList());

        var byVendor = pending
            .GroupBy(p => new { p.VendorId, VendorName = p.Vendor?.VendorProfile?.CompanyName ?? "N/A" })
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => (g.Key.VendorId, g.Key.VendorName, g.Count()))
            .ToList();

        return (stats.pendingCount, stats.oldestDate, stats.avgWaitDays, byVendor);
    }

    public async Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetProductUpdateRequestQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.ProductUpdateRequests
            .AsNoTracking()
            .Where(p => p.Status == ProductRegistrationStatus.Pending)
            .Select(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return CalculateQueueStats(pending);
    }

    public async Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetVendorCertificateQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.VendorCertificates
            .AsNoTracking()
            .Where(c => c.Status == VendorCertificateStatus.Pending)
            .Select(c => c.UploadedAt)
            .ToListAsync(cancellationToken);

        return CalculateQueueStats(pending);
    }

    public async Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetProductCertificateQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.ProductCertificates
            .AsNoTracking()
            .Where(c => c.Status == ProductCertificateStatus.Pending)
            .Select(c => c.UploadedAt)
            .ToListAsync(cancellationToken);

        return CalculateQueueStats(pending);
    }

    public async Task<(int pendingCount, int inReviewCount, DateTime? oldestDate, decimal avgWaitDays)> GetSupportRequestQueueAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(r => r.RequestType == RequestType.SupportRequest && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InReview))
            .ToListAsync(cancellationToken);

        var stats = CalculateQueueStats(requests.Select(r => r.CreatedAt).ToList());
        var inReviewCount = requests.Count(r => r.Status == RequestStatus.InReview);

        return (stats.pendingCount, inReviewCount, stats.oldestDate, stats.avgWaitDays);
    }

    public async Task<(int pendingCount, int inReviewCount, decimal totalAmount, DateTime? oldestDate, decimal avgWaitDays)> GetRefundRequestQueueAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(r => r.RequestType == RequestType.RefundRequest && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.InReview))
            .ToListAsync(cancellationToken);

        var stats = CalculateQueueStats(requests.Select(r => r.CreatedAt).ToList());
        var inReviewCount = requests.Count(r => r.Status == RequestStatus.InReview);
        
        // Note: totalAmount would need order details - simplified here
        return (stats.pendingCount, inReviewCount, 0, stats.oldestDate, stats.avgWaitDays);
    }

    public async Task<(int pendingCount, decimal totalAmount, DateTime? oldestDate, decimal avgWaitDays)> GetCashoutRequestQueueAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.WalletCashout && t.Status == TransactionStatus.Pending)
            .ToListAsync(cancellationToken);

        var stats = CalculateQueueStats(pending.Select(t => t.CreatedAt).ToList());
        var totalAmount = pending.Sum(t => t.Amount);

        return (stats.pendingCount, totalAmount, stats.oldestDate, stats.avgWaitDays);
    }

    #endregion

    #region Private Helpers

    private async Task<decimal> GetSystemRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionType == TransactionType.PaymentIn && t.Status == TransactionStatus.Completed)
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    private async Task<decimal> GetSystemCommissionAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= from && od.Order.CreatedAt < to)
            .SumAsync(od => od.Subtotal * od.Product.CommissionRate / 100, cancellationToken);
    }

    private static (int pendingCount, DateTime? oldestDate, decimal avgWaitDays) CalculateQueueStats(List<DateTime> dates)
    {
        if (dates.Count == 0)
            return (0, null, 0);

        var now = DateTime.UtcNow;
        var oldestDate = dates.Min();
        var avgWaitDays = (decimal)dates.Average(d => (now - d).TotalDays);

        return (dates.Count, oldestDate, Math.Round(avgWaitDays, 2));
    }

    #endregion
}

