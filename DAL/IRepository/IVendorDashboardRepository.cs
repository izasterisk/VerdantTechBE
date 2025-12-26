using DAL.Data.Models;

namespace DAL.IRepository;

public interface IVendorDashboardRepository
{
    // Overview
    Task<(decimal walletBalance, decimal pendingCashout)> GetWalletInfoAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(decimal thisMonth, decimal lastMonth)> GetMonthlyRevenueComparisonAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(int thisMonth, int lastMonth)> GetMonthlyOrderCountComparisonAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(int active, int outOfStock, int pendingRegistrations, int pendingUpdates)> GetProductCountsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(decimal averageRating, int totalReviews)> GetRatingInfoAsync(ulong vendorId, CancellationToken cancellationToken = default);
    
    // Revenue
    Task<(decimal grossRevenue, decimal commission, int orderCount)> GetRevenueByTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(DateOnly date, decimal grossRevenue, decimal commission, int orderCount)>> GetDailyRevenueAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(int month, decimal grossRevenue, decimal commission, int orderCount)>> GetMonthlyRevenueAsync(ulong vendorId, int year, CancellationToken cancellationToken = default);
    
    // Orders
    Task<Dictionary<string, int>> GetOrderCountsByStatusAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageDeliveryDaysAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    
    // Products
    Task<List<(ulong categoryId, string categoryName, int productCount)>> GetProductCategoryDistributionAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(int total, int active, int inactive, int outOfStock, int lowStock, int totalStock, decimal totalValue)> GetProductStatisticsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<(Product product, string? imageUrl, int soldQuantity, decimal totalRevenue)>> GetBestSellingProductsAsync(ulong vendorId, DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default);
    Task<(int star5, int star4, int star3, int star2, int star1)> GetRatingDistributionAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<(Product product, string? imageUrl, int reviewCount)>> GetTopRatedProductsAsync(ulong vendorId, int limit, bool ascending, CancellationToken cancellationToken = default);
    
    // Wallet
    Task<(decimal totalTopup, decimal totalCashout, int topupCount, int cashoutCount)> GetTransactionSummaryAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(decimal amount, int orderCount)> GetPendingCreditsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetRecentTransactionsAsync(ulong vendorId, int limit, CancellationToken cancellationToken = default);
    
    // Pending Items
    Task<List<ProductRegistration>> GetPendingProductRegistrationsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<(ProductUpdateRequest request, string productName)>> GetPendingProductUpdateRequestsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<VendorCertificate>> GetPendingVendorCertificatesAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<(ProductCertificate cert, ulong productId)>> GetPendingProductCertificatesAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetPendingCashoutRequestsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    
    // Transaction Export
    Task<List<Transaction>> GetAllTransactionsInTimeRangeAsync(ulong vendorId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

