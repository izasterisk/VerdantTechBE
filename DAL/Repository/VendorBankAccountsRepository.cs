using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class VendorBankAccountsRepository : IVendorBankAccountsRepository
{
    private readonly IRepository<VendorBankAccount> _vendorBankAccountRepository;
    private readonly VerdantTechDbContext _dbContext;

    public VendorBankAccountsRepository(VerdantTechDbContext context)
    {
        _vendorBankAccountRepository = new Repository<VendorBankAccount>(context);
        _dbContext = context;
    }

    public async Task<VendorBankAccount> CreateVendorBankAccountWithTransactionAsync(VendorBankAccount vendorBankAccount, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            vendorBankAccount.CreatedAt = DateTime.UtcNow;
            vendorBankAccount.UpdatedAt = DateTime.UtcNow;

            var createdAccount = await _vendorBankAccountRepository.CreateAsync(vendorBankAccount, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdAccount;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<VendorBankAccount> UpdateVendorBankAccountWithTransactionAsync(VendorBankAccount vendorBankAccount, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            vendorBankAccount.UpdatedAt = DateTime.UtcNow;
            var updatedAccount = await _vendorBankAccountRepository.UpdateAsync(vendorBankAccount, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedAccount;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteVendorBankAccountWithTransactionAsync(VendorBankAccount account, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await _vendorBankAccountRepository.DeleteAsync(account, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<VendorBankAccount> GetVendorBankAccountByIdAsync(ulong id, CancellationToken cancellationToken = default) =>
        await _vendorBankAccountRepository.GetAsync(vba => vba.Id == id, useNoTracking: true, cancellationToken) ??
        throw new KeyNotFoundException("Không tồn tại tài khoản ngân hàng với ID này.");

    public async Task<bool> ValidateImportedBankAccount(ulong vendorId, string accountNumber, string accountHolder,
        CancellationToken cancellationToken = default)
    {
        return await _vendorBankAccountRepository.AnyAsync(v => v.AccountNumber == accountNumber
            && string.Equals(v.AccountHolder, accountHolder, StringComparison.OrdinalIgnoreCase) 
            && v.VendorId == vendorId, cancellationToken);
    }
    
    public async Task<List<VendorBankAccount>> GetAllVendorBankAccountsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _vendorBankAccountRepository.GetAllByFilterAsync(
            vba => vba.VendorId == vendorId,
            useNoTracking: true,
            cancellationToken);
}