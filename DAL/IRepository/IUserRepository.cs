using DAL.Data.Models;

namespace DAL.IRepository;

public interface IUserRepository
{
    Task<User> CreateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<(List<User>, int totalCount)> GetAllUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
    Task<bool> CheckEmailExistsAsync(string username, CancellationToken cancellationToken = default);
}