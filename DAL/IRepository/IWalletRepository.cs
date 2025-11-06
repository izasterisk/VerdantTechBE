using DAL.Data.Models;

namespace DAL.IRepository;

public interface IWalletRepository
{
    Task<Wallet> CreateWalletWithTransactionAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet> UpdateWalletWithTransactionAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet?> GetWalletByVendorIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<bool> IsWalletExistsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
}