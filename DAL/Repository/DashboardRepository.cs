using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

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
    
    public async Task<decimal> GetTodayRevenueAsync()
    {
        var today = DateTime.Today;
        var todayPayments = await _paymentRepository.GetAllByFilterAsync(
            p => p.Status == PaymentStatus.Completed && 
                 p.CreatedAt >= today && p.CreatedAt < today.AddDays(1),
            useNoTracking: true
        );
        return todayPayments.Sum(p => p.Amount);
    }
}