using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        return await _userRepository.GetAsync(u =>u.Id == userId && u.Status == UserStatus.Active, useNoTracking: true);
    }
    
    public async Task<(List<User>, int totalCount)> GetAllUsersAsync(int page, int pageSize, String? role = null)
    {
        Expression<Func<User, bool>> filter = u => u.Status == UserStatus.Active;
        
        // Apply role filter
        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                filter = u => u.Status == UserStatus.Active && u.Role == userRole;
            }
        }
        else
        {
            // Default filter: only customers if no role specified
            filter = u => u.Status == UserStatus.Active && u.Role == UserRole.Customer;
        }

        return await _userRepository.GetPaginatedAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(u => u.UpdatedAt)
        );
    }
    
    
    public async Task<bool> CheckEmailExistsAsync(string username)
    {
        return await _userRepository.AnyAsync(u => u.Email.ToUpper() == username.ToUpper());
    }
}