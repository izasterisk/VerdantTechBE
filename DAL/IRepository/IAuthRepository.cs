using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(ulong userId);
    Task<bool> IsEmailExistsAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
}