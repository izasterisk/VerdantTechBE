using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class CashoutRepository : ICashoutRepository
{
    private readonly IRepository<Cashout> _cashoutRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IWalletRepository _walletRepository;
    
    public CashoutRepository(VerdantTechDbContext dbContext, IRepository<Cashout> cashoutRepository,
        IWalletRepository walletRepository)
    {
        _dbContext = dbContext;
        _cashoutRepository = cashoutRepository;
        _walletRepository = walletRepository;
    }

    public async Task<Cashout> CreateCashoutForWalletCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _walletRepository.GetWalletCashoutRequestByUserIdAsync(cashout.VendorId, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException(
                    "Yêu cầu rút tiền đang chờ xử lý, vui lòng chờ đến khi yêu cầu trước được xử lý. " +
                    "Mỗi tài khoản chỉ được tồn tại 1 yêu cầu chưa được xử lý.");
            cashout.CreatedAt = DateTime.UtcNow;
            cashout.UpdatedAt = DateTime.UtcNow;
            var result = await _cashoutRepository.CreateAsync(cashout, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Cashout> UpdateCashoutWithTransactionAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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

    public async Task<bool> DeleteCashoutAsync(Cashout cashout, CancellationToken cancellationToken = default)
    {
        return await _cashoutRepository.DeleteAsync(cashout, cancellationToken);
    }
}