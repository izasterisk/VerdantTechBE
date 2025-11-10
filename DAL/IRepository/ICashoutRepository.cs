using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICashoutRepository
{
    Task<Cashout> CreateCashoutForWalletCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
    Task<bool> DeleteCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
    Task<Cashout> UpdateCashoutWithTransactionAsync(Cashout cashout, CancellationToken cancellationToken = default);
}