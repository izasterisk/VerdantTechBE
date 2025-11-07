using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class WalletRepository : IWalletRepository
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<OrderDetail> _orderDetailRepository;

    public WalletRepository(VerdantTechDbContext context, IRepository<Wallet> walletRepository, IRepository<Order> orderRepository, IRepository<OrderDetail> orderDetailRepository)
    {
        _walletRepository = walletRepository;
        _orderRepository = orderRepository;
        _dbContext = context;
        _orderDetailRepository = orderDetailRepository;
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

    public async Task<Wallet> GetWalletByVendorIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetWithRelationsAsync(w => w.VendorId == vendorId,
            useNoTracking: true,
            query => query.Include(w => w.Vendor), cancellationToken);
        if (wallet == null)
        {
            var create = new Wallet
            {
                VendorId = vendorId,
                Balance = 0
            };
            var created = await CreateWalletWithTransactionAsync(create, cancellationToken);
            wallet = await _walletRepository.GetWithRelationsAsync(w => w.Id == created.Id,
                useNoTracking: true,
                query => query.Include(w => w.Vendor), cancellationToken);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Không thể tạo hoặc tải ví cho vendor ID {vendorId}");
            }
        }
        
        return wallet;
    }
        
    public async Task<Wallet> GetWalletByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var w = await _walletRepository.GetAsync(w => w.VendorId == vendorId, useNoTracking: true, cancellationToken);
        if (w == null)
        {
            var create = new Wallet();
            create.VendorId = vendorId;
            create.Balance = 0;
            w = await CreateWalletWithTransactionAsync(create, cancellationToken);
        }
        return w;
    }
    
    public async Task<bool> IsWalletExistsByVendorIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _walletRepository.AnyAsync(w => w.VendorId == vendorId, cancellationToken);

    public async Task<List<Order>> GetAllDeliveredOrdersAsync(CancellationToken cancellationToken = default) =>
        await _orderRepository.GetAllWithRelationsByFilterAsync(o => o.Status == OrderStatus.Delivered, 
            true, 
            includeFunc: query => query.Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product), cancellationToken);
    
}