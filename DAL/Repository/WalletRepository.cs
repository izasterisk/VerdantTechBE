using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class WalletRepository : IWalletRepository
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly VerdantTechDbContext _dbContext;

    public WalletRepository(VerdantTechDbContext context)
    {
        _walletRepository = new Repository<Wallet>(context);
        _dbContext = context;
    }

    public async Task<Wallet> CreateWalletWithTransactionAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            wallet.CreatedAt = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            var createdWallet = await _walletRepository.CreateAsync(wallet, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdWallet;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Wallet> UpdateWalletWithTransactionAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            wallet.UpdatedAt = DateTime.UtcNow;
            var updatedWallet = await _walletRepository.UpdateAsync(wallet, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedWallet;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Wallet?> GetWalletByVendorIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _walletRepository.GetWithRelationsAsync(
            w => w.VendorId == vendorId,
            useNoTracking: true,
            query => query.Include(w => w.Vendor),
            cancellationToken);
    
    public async Task<Wallet> GetWalletByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _walletRepository.GetAsync(w => w.VendorId == vendorId, useNoTracking: true, cancellationToken) ??
        throw new KeyNotFoundException("Ví của vendor này không tồn tại, hãy liên hệ staff.");
        
    
    public async Task<bool> IsWalletExistsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _walletRepository.AnyAsync(w => w.VendorId == vendorId, cancellationToken);
}