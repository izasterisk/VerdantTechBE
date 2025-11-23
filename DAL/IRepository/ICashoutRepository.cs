using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICashoutRepository
{
    Task<Cashout> CreateWalletCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
    Task<bool> DeleteCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
    Task<Cashout> CreateRefundCashoutWithTransactionAsync(Cashout cashout, Transaction tr, Order order, Request request, List<ProductSerial> serialIds, CancellationToken cancellationToken = default);
    Task<Cashout> UpdateCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default);
    Task<Cashout> GetCashoutRequestWithRelationsByIdAsync(ulong cashoutId, CancellationToken cancellationToken = default);
}