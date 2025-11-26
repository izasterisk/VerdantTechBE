using DAL.Data.Models;

namespace DAL.IRepository;

public interface IUserRepository
{
    Task<User> CreateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default);
    Task<User> CreateStaffWithTransactionAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithAddressesByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<(List<User>, int totalCount)> GetAllUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
    Task<bool> CheckEmailExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<User> GetVerifiedAndActiveUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task ValidateUserVerifiedAndActiveAsync(ulong userId, CancellationToken cancellationToken = default);
}