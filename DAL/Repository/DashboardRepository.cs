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
    public async Task<decimal> GetRevenueByTimeRangeAsync(DateOnly from, DateOnly to, ulong? vendorId, CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu ít nhất 1 ngày.", nameof(to));
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        if (vendorId == null)
        {
            return await _dbContext.Transactions
                .AsNoTracking()
                .Where(p => p.Status == TransactionStatus.Completed 
                            && p.TransactionType == TransactionType.PaymentIn 
                            && p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
                .SumAsync(p => p.Amount, cancellationToken);
        }
        
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.VendorId == vendorId.Value)
            .SelectMany(p => p.OrderDetails)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped 
                || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
            .SumAsync(od => od.Subtotal, cancellationToken);
    }
    
    public async Task<List<(Product product, MediaLink? image, int soldQuantity)>> GetTop5BestSellingProductsByTimeRangeAsync
        (DateOnly from, DateOnly to, ulong? vendorId, CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu ít nhất 1 ngày.", nameof(to));
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // Query order_details với order có trạng thái hợp lệ
        var query = _dbContext.OrderDetails
            .AsNoTracking()
            .Include(od => od.Product)
            .Where(od => od.Order.Status == OrderStatus.Paid || od.Order.Status == OrderStatus.Shipped 
                || od.Order.Status == OrderStatus.Delivered)
            .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime);

        // Filter theo vendor nếu có
        if (vendorId != null)
        {
            query = query.Where(od => od.Product.VendorId == vendorId.Value);
        }

        // Group theo product và tính tổng số lượng bán
        var topProducts = await query
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Product = g.First().Product,
                SoldQuantity = g.Sum(od => od.Quantity)
            })
            .OrderByDescending(x => x.SoldQuantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Lấy product IDs để query images
        var productIds = topProducts.Select(x => x.ProductId).ToList();

        // Query images cho các products (lấy ảnh đầu tiên sorted by sort_order)
        var productImages = await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.Products 
                         && productIds.Contains(ml.OwnerId))
            .GroupBy(ml => ml.OwnerId)
            .Select(g => new
            {
                ProductId = g.Key,
                FirstImage = g.OrderBy(ml => ml.SortOrder).First()
            })
            .ToListAsync(cancellationToken);

        // Map images vào dictionary để lookup O(1)
        var imageDict = productImages.ToDictionary(x => x.ProductId, x => x.FirstImage);

        return topProducts.Select(tp => (
            product: tp.Product,
            image: imageDict.TryGetValue(tp.ProductId, out var img) ? img : null,
            soldQuantity: tp.SoldQuantity
        )).ToList();
    }
    
    public async Task<(int VendorProfile, int ProductRegistration, int VendorCertificate, int ProductCertificate, int Request)> 
        GetNumberOfQueuesAsync(CancellationToken cancellationToken = default)
    {
        var vendorProfile = await _dbContext.VendorProfiles
            .AsNoTracking()
            .CountAsync(p => p.VerifiedAt == null && p.VerifiedBy == null, cancellationToken);
        var productRegistration = await _dbContext.ProductRegistrations
            .AsNoTracking()
            .CountAsync(p => p.Status == ProductRegistrationStatus.Pending, cancellationToken);
        var vendorCertificate = await _dbContext.VendorCertificates
            .AsNoTracking()
            .CountAsync(p => p.Status == VendorCertificateStatus.Pending, cancellationToken);
        var productCertificate = await _dbContext.ProductCertificates
            .AsNoTracking()
            .CountAsync(p => p.Status == ProductCertificateStatus.Pending, cancellationToken);
        var request = await _dbContext.Requests
            .AsNoTracking()
            .CountAsync(p => p.Status == RequestStatus.Pending, cancellationToken);
        return (
            VendorProfile: vendorProfile,
            ProductRegistration: productRegistration,
            VendorCertificate: vendorCertificate,
            ProductCertificate: productCertificate,
            Request: request
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
            .AsNoTracking()
            .Where(p => p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
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

    public async Task<Dictionary<DateOnly, decimal>> GetRevenueLast7DaysAsync(ulong? vendorId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sevenDaysAgo = today.AddDays(-6);
        
        var fromDateTime = sevenDaysAgo.ToDateTime(TimeOnly.MinValue);
        var toDateTime = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        List<dynamic> revenues;
        
        if (vendorId == null)
        {
            // Admin: Tổng doanh thu toàn hệ thống từ transactions
            revenues = await _dbContext.Transactions
                .AsNoTracking()
                .Where(p => p.TransactionType == TransactionType.PaymentIn
                            && p.Status == TransactionStatus.Completed
                            && p.CreatedAt >= fromDateTime && p.CreatedAt < toDateTime)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(p => p.Amount) })
                .ToListAsync<dynamic>(cancellationToken);
        }
        else
        {
            // Vendor: Doanh thu theo vendor từ order_details
            revenues = await _dbContext.OrderDetails
                .AsNoTracking()
                .Where(od => od.Product.VendorId == vendorId.Value)
                .Where(od => od.Order.Status == OrderStatus.Paid 
                             || od.Order.Status == OrderStatus.Shipped 
                             || od.Order.Status == OrderStatus.Delivered)
                .Where(od => od.Order.CreatedAt >= fromDateTime && od.Order.CreatedAt < toDateTime)
                .GroupBy(od => od.Order.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(od => od.Subtotal) })
                .ToListAsync<dynamic>(cancellationToken);
        }

        // Chuyển thành Dictionary để lookup O(1)
        var revenueDict = revenues.ToDictionary(
            r => DateOnly.FromDateTime(r.Date),
            r => (decimal)r.Revenue
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