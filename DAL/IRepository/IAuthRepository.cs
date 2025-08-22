using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task UpdateUserAsync(User user);
    Task LogoutUserAsync(User user);
}