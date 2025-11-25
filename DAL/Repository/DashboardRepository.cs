using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<ProductRegistration> _productRegistrationRepository;
    private readonly IRepository<VendorCertificate> _vendorCertificateRepository;
    private readonly IRepository<ProductCertificate> _productCertificateRepository;
    
    public DashboardRepository(IRepository<Wallet> walletRepository, IRepository<Order> orderRepository,
        VerdantTechDbContext dbContext, IRepository<VendorProfile> vendorProfileRepository,
        IRepository<Transaction> transactionRepository, IRepository<Request> requestRepository,
        IRepository<ProductRegistration> productRegistrationRepository, IRepository<VendorCertificate> vendorCertificateRepository,
        IRepository<ProductCertificate> productCertificateRepository)
    {
        _walletRepository = walletRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _vendorProfileRepository = vendorProfileRepository;
        _transactionRepository = transactionRepository;
        _requestRepository = requestRepository;
        _productRegistrationRepository = productRegistrationRepository;
        _vendorCertificateRepository = vendorCertificateRepository;
        _productCertificateRepository = productCertificateRepository;
    }
    public async Task<decimal> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu ít nhất 1 ngày.", nameof(to));
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return await _dbContext.Transactions
            .Where(p => p.Status == TransactionStatus.Completed 
                        && p.TransactionType == TransactionType.PaymentIn 
                        && p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
            .SumAsync(p => p.Amount, cancellationToken);
    }
    
    public async Task<(int VendorProfile, int ProductRegistration, int VendorCertificate, int ProductCertificate, int Request)> 
        GetNumberOfQueuesAsync(CancellationToken cancellationToken = default)
    {
        // Chạy 5 query song song để tối ưu performance
        var vendorProfileTask = _dbContext.VendorProfiles
            .CountAsync(p => p.VerifiedAt == null && p.VerifiedBy == null, cancellationToken);
        
        var productRegistrationTask = _dbContext.ProductRegistrations
            .CountAsync(p => p.Status == ProductRegistrationStatus.Pending, cancellationToken);
        
        var vendorCertificateTask = _dbContext.VendorCertificates
            .CountAsync(p => p.Status == VendorCertificateStatus.Pending, cancellationToken);
        
        var productCertificateTask = _dbContext.ProductCertificates
            .CountAsync(p => p.Status == ProductCertificateStatus.Pending, cancellationToken);
        
        var requestTask = _dbContext.Requests
            .CountAsync(p => p.Status == RequestStatus.Pending, cancellationToken);

        await Task.WhenAll(vendorProfileTask, productRegistrationTask, vendorCertificateTask, 
            productCertificateTask, requestTask);

        return (
            VendorProfile: vendorProfileTask.Result,
            ProductRegistration: productRegistrationTask.Result,
            VendorCertificate: vendorCertificateTask.Result,
            ProductCertificate: productCertificateTask.Result,
            Request: requestTask.Result
        );
    }
    
    public async Task<(int Total, int Paid, int Shipped, int Cancelled, int Delivered, int Refunded)> 
        GetNumberOfOrdersByTimeRangeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu ít nhất 1 ngày.", nameof(to));
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // Query một lần và group theo Status để tối ưu performance
        var statusCounts = await _dbContext.Orders
            .Where(p => p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var countDict = statusCounts.ToDictionary(x => x.Status, x => x.Count);

        return (
            Total: statusCounts.Sum(x => x.Count),
            Paid: countDict.GetValueOrDefault(OrderStatus.Paid),
            Shipped: countDict.GetValueOrDefault(OrderStatus.Shipped),
            Cancelled: countDict.GetValueOrDefault(OrderStatus.Cancelled),
            Delivered: countDict.GetValueOrDefault(OrderStatus.Delivered),
            Refunded: countDict.GetValueOrDefault(OrderStatus.Refunded)
        );
    }

    public async Task<Dictionary<DateOnly, decimal>> GetRevenueLast7DaysAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sevenDaysAgo = today.AddDays(-6);
        
        var fromDateTime = sevenDaysAgo.ToDateTime(TimeOnly.MinValue);
        var toDateTime = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // Thứ tự filter: TransactionType -> Status -> CreatedAt để tận dụng idx_type_status_created
        var revenues = await _dbContext.Transactions
            .Where(p => p.TransactionType == TransactionType.PaymentIn
                        && p.Status == TransactionStatus.Completed
                        && p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
            .GroupBy(p => p.CreatedAt.Date) // EF Core translate thành DATE(created_at)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(p => p.Amount) })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Chuyển thành Dictionary để lookup O(1)
        var revenueDict = revenues.ToDictionary(
            r => DateOnly.FromDateTime(r.Date),
            r => r.Revenue
        );

        // Tạo kết quả với pre-allocated capacity và lookup O(1)
        var result = new Dictionary<DateOnly, decimal>(7);
        for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
        {
            result[date] = revenueDict.TryGetValue(date, out var revenue) ? revenue : 0;
        }

        return result;
    }
}