using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class UserBankAccountsRepository : IUserBankAccountsRepository
{
    private readonly IRepository<UserBankAccount> _userBankAccountRepository;
    private readonly VerdantTechDbContext _dbContext;

    public UserBankAccountsRepository(VerdantTechDbContext context)
    {
        _userBankAccountRepository = new Repository<UserBankAccount>(context);
        _dbContext = context;
    }

    public async Task<UserBankAccount> CreateUserBankAccountWithTransactionAsync(UserBankAccount bankAccount, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            bankAccount.CreatedAt = DateTime.UtcNow;
            bankAccount.UpdatedAt = DateTime.UtcNow;

            var createdAccount = await _userBankAccountRepository.CreateAsync(bankAccount, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdAccount;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteUserBankAccountWithTransactionAsync(UserBankAccount account, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _userBankAccountRepository.DeleteAsync(account, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<UserBankAccount> GetUserBankAccountByIdAsync(ulong id, CancellationToken cancellationToken = default) =>
        await _userBankAccountRepository.GetAsync(uba => uba.Id == id, useNoTracking: true, cancellationToken) ??
        throw new KeyNotFoundException("Không tồn tại tài khoản ngân hàng với ID này.");

    public async Task<bool> ValidateImportedBankAccount(ulong userId, string accountNumber, string accountHolder,
        CancellationToken cancellationToken = default)
    {
        return await _userBankAccountRepository.AnyAsync(u => u.AccountNumber == accountNumber
            && string.Equals(u.AccountHolder, accountHolder, StringComparison.OrdinalIgnoreCase) 
            && u.UserId == userId, cancellationToken);
    }
    
    public async Task<List<UserBankAccount>> GetAllUserBankAccountsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await _userBankAccountRepository.GetAllByFilterAsync(
            uba => uba.UserId == userId,
            useNoTracking: true,
            cancellationToken);
}
