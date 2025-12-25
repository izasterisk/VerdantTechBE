using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    
    public PaymentRepository(IRepository<Payment> paymentRepository, IRepository<Order> orderRepository,
        VerdantTechDbContext dbContext, IRepository<Transaction> transactionRepository,
        IRepository<VendorProfile> vendorProfileRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _transactionRepository = transactionRepository;
        _vendorProfileRepository = vendorProfileRepository;
    }
    
    public async Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, Transaction tr, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            tr.CreatedAt = DateTime.UtcNow;
            tr.UpdatedAt = DateTime.UtcNow;
            var createdTransaction =  await _transactionRepository.CreateAsync(tr, cancellationToken);
            
            payment.TransactionId = createdTransaction.Id;
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            var createdPayment = await _paymentRepository.CreateAsync(payment, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdPayment;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Payment> UpdateFullPaymentWithTransactionAsync(Payment payment, Order? order, Transaction transactions, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if(order != null)
            {
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order, cancellationToken);
            }

            if (transactions.Note is "12MONTHS" or "6MONTHS")
            {
                var v = await _vendorProfileRepository.GetAsync
                    (p => p.UserId == transactions.UserId, true, cancellationToken);
                if (v != null)
                {
                    v.SubscriptionActive = true;
                    await _vendorProfileRepository.UpdateAsync(v, cancellationToken);
                }
            }
            
            transactions.UpdatedAt = DateTime.UtcNow;
            await _transactionRepository.UpdateAsync(transactions, cancellationToken);
            
            payment.UpdatedAt = DateTime.UtcNow;
            var updatedPayment = await _paymentRepository.UpdateAsync(payment, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return updatedPayment;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Transaction> GetPaymentWithRelationByTransactionIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetWithRelationsAsync(p => p.Id == id, true, 
            query => query
                .Include(p => p.Payment)
                .Include(t => t.User)
                .Include(t => t.ProcessedBy)
                .Include(t => t.CreatedBy), cancellationToken) ?? 
               throw new KeyNotFoundException($"Không tìm thấy giao dịch với ID {id}.");
    }
}