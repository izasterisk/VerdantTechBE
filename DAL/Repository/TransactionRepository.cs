using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class TransactionRepository : ITransactionRepository
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public TransactionRepository(IRepository<Transaction> transactionRepository, VerdantTechDbContext dbContext)
    {
        _transactionRepository = transactionRepository;
        _dbContext = dbContext;
    }
    
    public async Task<Transaction> GetTransactionForPaymentByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetWithRelationsAsync(
                   t => t.GatewayPaymentId != null && t.GatewayPaymentId.Equals(gatewayPaymentId, StringComparison.OrdinalIgnoreCase), 
                   true, 
                   query => query.Include(t => t.Payment)
                       .Include(t => t.Order),
                   cancellationToken) ?? 
               throw new KeyNotFoundException($"Không tồn tại giao dịch với mã thanh toán {gatewayPaymentId}.");
    }
    
    public async Task<Transaction> GetTransactionByOrderIdAsync(ulong orderId, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetAsync(
                   t => t.OrderId == orderId, true, cancellationToken) ?? 
               throw new KeyNotFoundException($"Không tồn tại giao dịch cho đơn hàng với Id {orderId}.");
    }
}