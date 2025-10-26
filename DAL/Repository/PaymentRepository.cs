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
    private readonly ITransactionRepository _transactionRepository;
    
    public PaymentRepository(IRepository<Payment> paymentRepository, IRepository<Order> orderRepository,
        VerdantTechDbContext dbContext, ITransactionRepository transactionRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _transactionRepository = transactionRepository;
    }
    
    public async Task<Payment> CreatePaymentWithTransactionAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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
    
    public async Task<Payment> UpdateFullPaymentWithTransactionAsync(Payment payment, Order order, Transaction transactions, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            payment.UpdatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _transactionRepository.CreateTransactionAsync(transactions, cancellationToken);
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
    
    public async Task<Payment?> GetPaymentByGatewayPaymentIdAsync(string paymentGatewayId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetWithRelationsAsync(
            filter: p => p.GatewayPaymentId == paymentGatewayId,
            useNoTracking: true, includeFunc: q => q.Include(p => p.Order),
            cancellationToken: cancellationToken);
    }
}