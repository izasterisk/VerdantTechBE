using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
}