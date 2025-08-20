using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly IRepository<User> _userRepository;

    public AuthRepository(VerdantTechDbContext context)
    {
        _userRepository = new Repository<User>(context);
    }
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository
            .GetAsync(u => u.Email.ToLower() == email.ToLower() && u.DeletedAt == null && u.Status != UserStatus.Deleted);
    }
}