using DAL.Data.Models;

namespace DAL.IRepository;

public interface ITransactionRepository
{
    Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
}