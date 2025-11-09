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
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly ITransactionRepository _transactionRepository;
    
    public WalletRepository(IRepository<Wallet> walletRepository, IRepository<Order> orderRepository,
        VerdantTechDbContext dbContext, IRepository<OrderDetail> orderDetailRepository,
        IRepository<VendorProfile> vendorProfileRepository, IRepository<Cashout> cashoutRepository,
        ITransactionRepository transactionRepository)
    {
        _walletRepository = walletRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _orderDetailRepository = orderDetailRepository;
        _vendorProfileRepository = vendorProfileRepository;
        _cashoutRepository = cashoutRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Wallet> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        wallet.CreatedAt = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;

        return await _walletRepository.CreateAsync(wallet, cancellationToken);
    }

    public async Task<Wallet> UpdateWalletAndOrderDetailsWithTransactionAsync(List<OrderDetail> orderDetails, Wallet wallet, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var orderDetail in orderDetails)
            {
                orderDetail.UpdatedAt = DateTime.UtcNow;
                await _orderDetailRepository.UpdateAsync(orderDetail, cancellationToken);
            }
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
    
    public async Task<Cashout> ProcessWalletCashoutRequestAsync(Transaction tr, Cashout cashout, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var t = await _transactionRepository.CreateTransactionAsync(tr, cancellationToken);
            
            cashout.TransactionId = t.Id;
            cashout.UpdatedAt = DateTime.UtcNow;
            var c = await _cashoutRepository.UpdateAsync(cashout, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return c;
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
                Balance = 0
            };
            var created = await CreateWalletAsync(create, cancellationToken);
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
        var w = await _walletRepository.GetAsync(w => w.VendorId == vendorId, useNoTracking: false, cancellationToken);
        if (w == null)
        {
            var create = new Wallet();
            create.VendorId = vendorId;
            create.Balance = 0;
            w = await CreateWalletAsync(create, cancellationToken);
        }
        return w;
    }
    
    public async Task<List<OrderDetail>> GetAllOrderDetailsAvailableForCreditAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _orderDetailRepository.GetAllWithRelationsByFilterAsync(o => o.IsWalletCredited == false 
            && o.UpdatedAt.AddDays(7) < DateTime.UtcNow && o.Product.VendorId == vendorId, 
            false, 
            includeFunc: query => query.Include(od => od.Product), cancellationToken);
    
    public async Task<bool> ValidateVendorQualified(ulong userId, CancellationToken cancellationToken = default) =>
        await _vendorProfileRepository.AnyAsync(w => w.UserId == userId && w.VerifiedAt != null 
            && w.VerifiedBy != null, cancellationToken);
    
    public async Task<Cashout?> GetWalletCashoutRequestByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _cashoutRepository.GetAsync(c => c.VendorId == vendorId && c.Status == CashoutStatus.Processing
            && c.ProcessedAt == null && c.ProcessedBy == null 
            && c.ReferenceType == CashoutReferenceType.VendorWithdrawal, true, cancellationToken);
    
    public async Task<Cashout?> GetWalletCashoutRequestWithRelationsByUserIdAsync(ulong vendorId, CancellationToken cancellationToken = default) =>
        await _cashoutRepository.GetWithRelationsAsync(c => c.VendorId == vendorId && c.Status == CashoutStatus.Processing 
            && c.ProcessedAt == null && c.ProcessedBy == null 
            && c.ReferenceType == CashoutReferenceType.VendorWithdrawal, true, 
            query => query.Include(u => u.BankAccount)
                .ThenInclude(u => u.User), cancellationToken);
    
    public async Task<Cashout?> GetWalletCashoutRequestWithRelationsByIdAsync(ulong cashoutId, CancellationToken cancellationToken = default) =>
        await _cashoutRepository.GetWithRelationsAsync(c => c.Id == cashoutId, true, 
            query => query.Include(u => u.Vendor)
                .Include(u => u.Transaction)
                .Include(u => u.BankAccount)
                .Include(u => u.ProcessedByNavigation), cancellationToken);
    
    public async Task<(List<Cashout>, int totalCount)> GetAllWalletCashoutRequestAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _cashoutRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            c => c.Status == CashoutStatus.Processing 
                && c.ProcessedAt == null && c.ProcessedBy == null 
                && c.ReferenceType == CashoutReferenceType.VendorWithdrawal,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(c => c.CreatedAt),
            includeFunc: query => query.Include(c => c.BankAccount).ThenInclude(u => u.User),
            cancellationToken
        );
    }
}