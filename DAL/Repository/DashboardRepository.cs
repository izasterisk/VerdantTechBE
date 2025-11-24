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
    private readonly IRepository<OrderDetail> _orderDetailRepository;
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IRepository<Payment> _paymentRepository;
    
    public DashboardRepository(IRepository<Wallet> walletRepository, IRepository<Order> orderRepository,
        VerdantTechDbContext dbContext, IRepository<OrderDetail> orderDetailRepository,
        IRepository<VendorProfile> vendorProfileRepository, IRepository<Cashout> cashoutRepository,
        ITransactionRepository transactionRepository, IRepository<Payment> paymentRepository)
    {
        _walletRepository = walletRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _orderDetailRepository = orderDetailRepository;
        _vendorProfileRepository = vendorProfileRepository;
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
        _paymentRepository = paymentRepository;
    }
    
    public async Task<decimal> GetRevenueByTimeAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (from > to)
            throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu ít nhất 1 ngày.", nameof(to));
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        return await _dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Completed && 
                        p.CreatedAt >= fromDateTime && 
                        p.CreatedAt < toDateTime)
            .SumAsync(p => p.Amount, cancellationToken);
    }
}