using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly IRepository<User> _userRepository;
    private readonly VerdantTechDbContext _context;

    public AuthRepository(VerdantTechDbContext context)
    {
        _userRepository = new Repository<User>(context);
        _context = context;
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetAsync(u => u.Email.ToLower() == email.ToLower() 
                                                   && u.DeletedAt == null && u.Status != UserStatus.Deleted);
    }

    public async Task<User?> GetUserByIdAsync(ulong id)
    {
        return await _userRepository.GetAsync(u => u.Id == id 
                                                   && u.DeletedAt == null && u.Status != UserStatus.Deleted);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _userRepository.GetAsync(u => u.RefreshToken == refreshToken 
                                                   && u.RefreshTokenExpiresAt > DateTime.UtcNow
                                                   && u.DeletedAt == null && u.Status != UserStatus.Deleted);
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _context.SaveChangesAsync();
    }
}