using DAL.Data.Models;

namespace DAL.IRepository;

public interface IVendorBankAccountsRepository
{
    Task<VendorBankAccount> CreateVendorBankAccountWithTransactionAsync(VendorBankAccount vendorBankAccount, CancellationToken cancellationToken = default);
    Task<VendorBankAccount> UpdateVendorBankAccountWithTransactionAsync(VendorBankAccount vendorBankAccount, CancellationToken cancellationToken = default);
    Task<List<VendorBankAccount>> GetAllVendorBankAccountsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default);
    Task<VendorBankAccount> GetVendorBankAccountByIdAsync(ulong id, CancellationToken cancellationToken = default);
}