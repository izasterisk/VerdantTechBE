using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAdminDashboardRepository
{
    // Overview
    Task<(decimal today, decimal thisWeek, decimal thisMonth, decimal lastMonth)> GetRevenueOverviewAsync(CancellationToken cancellationToken = default);
    Task<(decimal thisMonth, decimal lastMonth)> GetCommissionOverviewAsync(CancellationToken cancellationToken = default);
    Task<(int today, int thisWeek, int thisMonth, int pendingShipment, int inTransit)> GetOrdersOverviewAsync(CancellationToken cancellationToken = default);
    Task<(int totalCustomers, int totalVendors, int newCustomersThisMonth, int newVendorsThisMonth)> GetUsersOverviewAsync(CancellationToken cancellationToken = default);
    Task<(int active, int inactive, int outOfStock)> GetProductsOverviewAsync(CancellationToken cancellationToken = default);
    Task<(int vendorProfiles, int productRegistrations, int vendorCertificates, int productCertificates, 
          int productUpdateRequests, int supportRequests, int refundRequests, int cashoutRequests)> GetPendingQueuesOverviewAsync(CancellationToken cancellationToken = default);
    
    // Revenue
    Task<(decimal totalRevenue, decimal totalCommission, int totalOrders, int totalTransactions)> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int bankingCount, decimal bankingAmount, int codCount, decimal codAmount)> GetPaymentMethodBreakdownAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(DateOnly date, decimal revenue, decimal commission, int orderCount, int transactionCount)>> GetDailyRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(int month, decimal revenue, decimal commission, int orderCount)>> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default);
    
    // Orders
    Task<Dictionary<string, int>> GetOrderCountsByStatusAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int bankingCount, int codCount)> GetOrderCountsByPaymentMethodAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(int courierId, int orderCount)>> GetOrderCountsByCourierAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageDeliveryDaysAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    
    // Users
    Task<(int total, int active, int inactive, int newThisPeriod)> GetCustomerStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int total, int verified, int pending, int newThisPeriod)> GetVendorStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int total, int active)> GetStaffStatisticsAsync(CancellationToken cancellationToken = default);
    Task<List<(DateOnly date, int customers, int vendors)>> GetRegistrationTrendAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    
    // Products
    Task<(int total, int active, int inactive, int outOfStock, int lowStock)> GetProductStatisticsAsync(CancellationToken cancellationToken = default);
    Task<List<(ulong categoryId, string categoryName, int productCount, int activeCount, int outOfStockCount)>> GetCategoryDistributionAsync(CancellationToken cancellationToken = default);
    Task<(int vendorsWithProducts, decimal avgProductsPerVendor, int topVendorProductCount)> GetVendorProductDistributionAsync(CancellationToken cancellationToken = default);
    Task<List<(Product product, string vendorName, string? imageUrl, int soldQuantity, decimal totalRevenue, decimal commission)>> GetBestSellingProductsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default);
    
    // Vendors
    Task<(int total, int verified, int pending, int active, int inactive, int suspended)> GetVendorCountsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalVendorRevenueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(VendorProfile vendor, decimal grossRevenue, decimal commission, int orderCount, int productCount, decimal avgRating, decimal walletBalance)>> GetTopVendorsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default);
    
    // Transactions
    Task<List<Transaction>> GetAllTransactionsInTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(List<Transaction> transactions, int totalCount)> GetTransactionsWithPagingAsync(DateOnly from, DateOnly to, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(decimal totalInflow, decimal totalOutflow)> GetTransactionFlowAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int count, decimal completedAmount, decimal pendingAmount, int failedCount)> GetPaymentInStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int count, decimal completedAmount)> GetWalletTopupStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int count, decimal completedAmount, int pendingCount, decimal pendingAmount)> GetWalletCashoutStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<(int count, decimal completedAmount, int pendingCount, decimal pendingAmount)> GetRefundStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<List<(DateOnly date, decimal inflow, decimal outflow, int count)>> GetDailyTransactionTrendAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    
    // Queue Statistics
    Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetVendorProfileQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays, List<(ulong vendorId, string vendorName, int count)> byVendor)> GetProductRegistrationQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetProductUpdateRequestQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetVendorCertificateQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, DateTime? oldestDate, decimal avgWaitDays)> GetProductCertificateQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, int inReviewCount, DateTime? oldestDate, decimal avgWaitDays)> GetSupportRequestQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, int inReviewCount, decimal totalAmount, DateTime? oldestDate, decimal avgWaitDays)> GetRefundRequestQueueAsync(CancellationToken cancellationToken = default);
    Task<(int pendingCount, decimal totalAmount, DateTime? oldestDate, decimal avgWaitDays)> GetCashoutRequestQueueAsync(CancellationToken cancellationToken = default);
}

