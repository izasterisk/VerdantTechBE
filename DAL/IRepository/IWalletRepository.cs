using DAL.Data.Models;

namespace DAL.IRepository;

public interface IWalletRepository
{
    Task<Wallet> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task<Cashout> ProcessWalletCashoutRequestAsync(Transaction tr, Cashout cashout, CancellationToken cancellationToken = default);
    Task<Wallet> UpdateWalletAndOrderDetailsWithTransactionAsync(List<OrderDetail> orderDetails, Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<OrderDetail>> GetAllOrderDetailsAvailableForCreditAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<bool> ValidateVendorQualified(ulong userId, CancellationToken cancellationToken = default);
    Task<Cashout?> GetWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Cashout?> GetWalletCashoutRequestWithRelationsByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<(List<Cashout>, int totalCount)> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Cashout?> GetWalletCashoutRequestWithRelationsByIdAsync(ulong cashoutId, CancellationToken cancellationToken = default);
}