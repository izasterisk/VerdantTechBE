using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CashoutRepository : ICashoutRepository
{
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<ProductSerial> _productSerialRepository; 
    private readonly IRepository<Request> _requestRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    
    public CashoutRepository(IRepository<Cashout> cashoutRepository, IRepository<Transaction> transactionRepository,
        IRepository<Order> orderRepository, IRepository<Payment> paymentRepository,
        IRepository<ProductSerial> productSerialRepository, IRepository<Request> requestRepository,
        VerdantTechDbContext dbContext, IWalletRepository walletRepository)
    {
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _productSerialRepository = productSerialRepository;
        _requestRepository = requestRepository;
        _dbContext = dbContext;
        _walletRepository = walletRepository;
    }

    public async Task<Cashout> CreateWalletCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(tr.UserId, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException(
                    "Yêu cầu rút tiền đang chờ xử lý, vui lòng chờ đến khi yêu cầu trước được xử lý. " +
                    "Mỗi tài khoản chỉ được tồn tại 1 yêu cầu chưa được xử lý.");
            
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            cashout.TransactionId = createdTransaction.Id;
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            var result = await _cashoutRepository.CreateAsync(cashout, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Cashout> UpdateCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        cashout.UpdatedAt = DateTime.UtcNow;
        return await _cashoutRepository.UpdateAsync(cashout, cancellationToken);
    }

    public async Task<Cashout> CreateRefundCashoutWithTransactionAsync(Cashout cashout, Transaction tr, 
        Order order, Request request, List<ProductSerial> serialIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            order.UpdatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Refunded;
            await _orderRepository.UpdateAsync(order, cancellationToken);
            
            request.UpdatedAt = DateTime.UtcNow;
            request.Status = RequestStatus.Completed;
            await _requestRepository.UpdateAsync(request, cancellationToken);
            
            var payment = await _paymentRepository.GetAsync(u => u.OrderId == order.Id, true, cancellationToken) ?? 
                throw new KeyNotFoundException("Không tìm thấy thanh toán liên quan đến đơn hàng.");
            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Chỉ có thể hoàn tiền cho các thanh toán đã hoàn tất.");
            payment.UpdatedAt = DateTime.UtcNow;
            payment.Status = PaymentStatus.Refunded;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            foreach (var serialId in serialIds)
            {
                serialId.Status = ProductSerialStatus.Refund;
                serialId.UpdatedAt = DateTime.UtcNow;
                await _productSerialRepository.UpdateAsync(serialId, cancellationToken);
            }
            
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            cashout.TransactionId = createdTransaction.Id;
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            var result = await _cashoutRepository.CreateAsync(cashout, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Cashout> GetCashoutRequestWithRelationsByIdAsync(ulong cashoutId, CancellationToken cancellationToken = default) =>
        await _cashoutRepository.GetWithRelationsAsync(c => c.Id == cashoutId, true, 
            query => query.Include(u => u.User)
                .Include(u => u.Transaction)
                .Include(u => u.BankAccount)
                .Include(u => u.ProcessedByNavigation), cancellationToken) ?? 
        throw new KeyNotFoundException("Yêu cầu rút tiền không tồn tại.");

    public async Task<bool> DeleteCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        return await _cashoutRepository.DeleteAsync(cashout, cancellationToken);
    }
}