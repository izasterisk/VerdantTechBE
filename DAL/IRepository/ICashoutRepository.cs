using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICashoutRepository
{
    Task<Cashout> CreateWalletCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default);
    Task<Transaction> CreateRefundCashoutWithTransactionAsync(Cashout cashout, Transaction tr, Order order, Request request, List<ProductSerial> serialIds, CancellationToken cancellationToken = default);
    Task<Transaction> UpdateCashoutAsync(Cashout cashout, Transaction tr, CancellationToken cancellationToken = default);
    Task<Transaction> GetCashoutRequestWithRelationsByTransactionIdAsync(ulong transactionId, CancellationToken cancellationToken = default);
}