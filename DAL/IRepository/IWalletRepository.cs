using DAL.Data.Models;

namespace DAL.IRepository;

public interface IWalletRepository
{
    Task<Wallet> CreateWalletWithTransactionAsync(Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet> UpdateWalletAndOrderDetailsWithTransactionAsync(List<OrderDetail> orderDetails, Wallet wallet, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<Wallet> GetWalletByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<List<OrderDetail>> GetAllOrderDetailsAvailableForCreditAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<bool> ValidateVendorQualified(ulong userId, CancellationToken cancellationToken = default);
}