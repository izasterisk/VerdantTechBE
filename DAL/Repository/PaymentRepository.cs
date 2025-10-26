using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly ITransactionRepository _transactionRepository;
    
    public PaymentRepository(IRepository<Payment> paymentRepository, VerdantTechDbContext dbContext, ITransactionRepository transactionRepository)
    {
        _paymentRepository = paymentRepository;
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
}