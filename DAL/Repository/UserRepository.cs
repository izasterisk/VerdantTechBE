using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class UserRepository : IUserRepository
{
    private readonly IRepository<User> _userRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public UserRepository(VerdantTechDbContext context)
    {
        _userRepository = new Repository<User>(context);
        _dbContext = context;
    }
    
    public async Task<User> CreateUserWithTransactionAsync(User user)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            user.LastLoginAt = DateTime.Now;
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            var createdUser = await _userRepository.CreateAsync(user);
            await transaction.CommitAsync();
            return createdUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> UpdateUserWithTransactionAsync(User user)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            user.UpdatedAt = DateTime.Now;
            var updatedUser = await _userRepository.UpdateAsync(user);
            await transaction.CommitAsync();
            return updatedUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<User?> GetUserByIdAsync(ulong userId)
    {
        return await _userRepository.GetAsync(u =>u.Id == userId && u.Role == UserRole.Customer && u.Status == UserStatus.Active, useNoTracking: true);
    }
    
    public async Task<(List<User> users, int totalCount)> GetAllUsersAsync(int page, int pageSize)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Customer && u.Status == UserStatus.Active)
            .OrderByDescending(u => u.UpdatedAt);

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }
    
    public async Task<bool> CheckEmailExistsAsync(string username)
    {
        return await _userRepository.AnyAsync(u => u.Email.Equals(username));
    }
}