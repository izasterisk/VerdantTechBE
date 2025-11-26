using DAL.Data.Models;

namespace DAL.IRepository;

public interface IUserBankAccountsRepository
{
    Task<UserBankAccount> CreateUserBankAccountWithTransactionAsync(UserBankAccount userBankAccount, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteUserBankAccountWithTransactionAsync(UserBankAccount account, CancellationToken cancellationToken = default);
    Task<List<UserBankAccount>> GetAllUserBankAccountsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<UserBankAccount> GetUserBankAccountByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task IsUserAlreadyHaveBankAccount(ulong userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateImportedBankAccount(ulong userId, string bankCode, string accountNumber, CancellationToken cancellationToken = default);
}
