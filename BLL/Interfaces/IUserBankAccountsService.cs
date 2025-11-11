using BLL.DTO.UserBankAccount;

namespace BLL.Interfaces;

public interface IUserBankAccountsService
{
    Task<UserBankAccountResponseDTO> CreateUserBankAccountAsync(ulong userId, UserBankAccountCreateDTO dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserBankAccountAsync(ulong accountId, CancellationToken cancellationToken = default);
    Task<List<UserBankAccountResponseDTO>> GetAllUserBankAccountsByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}
