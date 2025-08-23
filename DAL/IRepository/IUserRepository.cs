using DAL.Data.Models;

namespace DAL.IRepository;

public interface IUserRepository
{
    Task<User> CreateUserWithTransactionAsync(User user);
    Task<User> UpdateUserWithTransactionAsync(User user);
    Task<User?> GetUserByIdAsync(ulong userId);
    Task<(List<User> users, int totalCount)> GetAllUsersAsync(int page, int pageSize);
    Task<bool> CheckEmailExistsAsync(string username);
}