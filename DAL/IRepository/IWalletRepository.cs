using DAL.Data.Models;

namespace DAL.IRepository;

public interface IWalletRepository
{
    Task<bool> DeleteCashoutWithTransactionAsync(Transaction tr, Cashout cashout, CancellationToken cancellationToken = default);
    Task<Transaction?> GetTransactionWithWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Transaction> ProcessWalletCashoutRequestWithTransactionAsync(Transaction tr, Cashout cashout, Wallet wallet, UserBankAccount bank, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<Order>> GetAllOrdersAvailableForCreditAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateVendorQualified(ulong userId, CancellationToken cancellationToken = default);
    Task<Transaction?> GetWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Transaction> GetWalletCashoutRequestWithRelationsByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(List<Transaction>, int totalCount)> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Transaction>, int totalCount)> GetAllWalletCashoutRequestByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task ProcessWalletTopUpAsync(List<Order> orders, List<Transaction> transactions, Dictionary<ulong, decimal> walletsToUpdate, CancellationToken cancellationToken = default);
}