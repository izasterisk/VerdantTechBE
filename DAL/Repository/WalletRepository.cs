using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class WalletRepository : IWalletRepository
{
    private readonly IRepository<Wallet> _walletRepository;
    private readonly IRepository<UserBankAccount> _userBankAccountRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Order> _orderRepository;
    
    public WalletRepository(IRepository<Wallet> walletRepository, IRepository<UserBankAccount> userBankAccountRepository,
        VerdantTechDbContext dbContext, IRepository<VendorProfile> vendorProfileRepository,
        IRepository<Cashout> cashoutRepository, IRepository<Transaction> transactionRepository,
        IRepository<Order> orderRepository)
    {
        _walletRepository = walletRepository;
        _userBankAccountRepository = userBankAccountRepository;
        _dbContext = dbContext;
        _vendorProfileRepository = vendorProfileRepository;
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
        _orderRepository = orderRepository;
    }

    public async Task ProcessWalletTopUpAsync(List<Order> orders, 
        List<Transaction> transactions, Dictionary<ulong, decimal> walletsToUpdate, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _orderRepository.BulkUpdateAsync(orders, cancellationToken);
            
            var wallets = new List<Wallet>();
            foreach (var walletToUpdate in walletsToUpdate)
            {
                var w = await GetWalletByUserIdAsync(walletToUpdate.Key, cancellationToken);
                w.Balance += walletToUpdate.Value;
                w.LastUpdatedBy = 1;
                w.UpdatedAt = DateTime.UtcNow;
                wallets.Add(w);
            }
            await _walletRepository.BulkUpdateAsync(wallets, cancellationToken);

            await _transactionRepository.CreateBulkAsync(transactions, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Transaction> ProcessWalletCashoutRequestWithTransactionAsync(Transaction tr, Cashout cashout, 
        Wallet wallet, UserBankAccount bank, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            tr.UpdatedAt = DateTime.UtcNow;
            var t = await _transactionRepository.UpdateAsync(tr, cancellationToken);
            
            wallet.UpdatedAt = DateTime.UtcNow;
            await _walletRepository.UpdateAsync(wallet, cancellationToken);
            
            cashout.UpdatedAt = DateTime.UtcNow;
            await _cashoutRepository.UpdateAsync(cashout, cancellationToken);
            
            bank.UpdatedAt = DateTime.UtcNow;
            await _userBankAccountRepository.UpdateAsync(bank, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return t;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Wallet> GetWalletByUserIdWithRelationsAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var wallet = await _walletRepository.GetWithRelationsAsync(w => w.VendorId == vendorId,
            useNoTracking: false,
            query => query.Include(w => w.Vendor), cancellationToken);
        if (wallet == null)
        {
            var create = new Wallet
            {
                VendorId = vendorId,
                Balance = 0,
                LastUpdatedBy = vendorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await _walletRepository.CreateAsync(create, cancellationToken);
            wallet = await _walletRepository.GetWithRelationsAsync(w => w.Id == created.Id,
                useNoTracking: false,
                query => query.Include(w => w.Vendor), cancellationToken);
            if (wallet == null)
            {
                throw new InvalidOperationException($"Không thể tạo hoặc tải ví cho vendor ID {vendorId}");
            }
        }
        return wallet;
    }
        
    public async Task<Wallet> GetWalletByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default)
    {
        var w = await _walletRepository.GetAsync(w => w.VendorId == vendorId, useNoTracking: true, cancellationToken);
        if (w == null)
        {
            var create = new Wallet
            {
                VendorId = vendorId,
                Balance = 0,
                LastUpdatedBy = vendorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            w = await _walletRepository.CreateAsync(create, cancellationToken);
        }
        return w;
    }
    
    public async Task<List<Order>> GetAllOrdersAvailableForCreditAsync(CancellationToken cancellationToken = default)
    { 
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        return await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(o => o.Product)
            .Where(o => o.IsWalletCredited == false)
            .Where(o => o.Status == OrderStatus.Delivered && o.DeliveredAt != null
                && o.DeliveredAt < sevenDaysAgo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
        
    public async Task<bool> ValidateVendorQualified(ulong userId, CancellationToken cancellationToken = default) =>
        await _vendorProfileRepository.AnyAsync(w => w.UserId == userId && w.VerifiedAt != null 
            && w.VerifiedBy != null, cancellationToken);
    
    public async Task<Transaction?> GetWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _transactionRepository.GetAsync(c => c.UserId == vendorId 
            && c.TransactionType == TransactionType.WalletCashout
            && c.Status == TransactionStatus.Pending, true, cancellationToken);
    
    public async Task<Transaction?> GetTransactionWithWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _transactionRepository.GetWithRelationsAsync(c => c.UserId == vendorId 
           && c.TransactionType == TransactionType.WalletCashout
           && c.Status == TransactionStatus.Pending, true, 
            query => query.Include(u => u.Cashout)
                .Include(u => u.BankAccount), cancellationToken);
    
    public async Task<Transaction> GetWalletCashoutRequestWithRelationsByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _transactionRepository.GetWithRelationsAsync(c => c.UserId == vendorId 
            && c.TransactionType == TransactionType.WalletCashout 
            && c.Status == TransactionStatus.Pending, true, 
            query => query.Include(u => u.BankAccount)
                .Include(u => u.CreatedByNavigation)
                .Include(u => u.User)
                .Include(u => u.Cashout), cancellationToken)
        ?? throw new KeyNotFoundException($"Yêu cầu rút tiền của vendor ID {vendorId} không tồn tại.");
    
    public async Task<bool> DeleteCashoutWithTransactionAsync(Transaction tr, Cashout cashout, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _cashoutRepository.DeleteAsync(cashout, cancellationToken);
            await _transactionRepository.DeleteAsync(tr, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<(List<Transaction>, int totalCount)> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            c => c.TransactionType == TransactionType.WalletCashout && c.Status == TransactionStatus.Pending,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(c => c.CreatedAt),
            includeFunc: query => query.Include(u => u.BankAccount)
                .Include(u => u.CreatedByNavigation)
                .Include(u => u.User)
                .Include(u => u.Cashout),
            cancellationToken
        );
    }
    
    public async Task<(List<Transaction>, int totalCount)> GetAllWalletCashoutRequestByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _transactionRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            c => c.UserId == userId && c.TransactionType == TransactionType.WalletCashout,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(c => c.CreatedAt),
            includeFunc: query => query.Include(u => u.BankAccount)
                .Include(u => u.CreatedByNavigation)
                .Include(u => u.ProcessedByNavigation)
                .Include(u => u.User)
                .Include(u => u.Cashout),
            cancellationToken
        );
    }
}