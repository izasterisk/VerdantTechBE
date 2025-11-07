using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICashoutRepository
{
    Task<Cashout> CreateCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
}