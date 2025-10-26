using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

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
    
    public async Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        var createdTransaction = await _transactionRepository.CreateAsync(transaction, cancellationToken);
        return createdTransaction;
    }
}