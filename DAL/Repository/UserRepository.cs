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
    
    public async Task<User> CreateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.Status = UserStatus.Active;
            user.Role = UserRole.Customer;
            
            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<User> UpdateUserWithTransactionAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<User> CreateStaffWithTransactionAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.Status = UserStatus.Active;
            user.Role = UserRole.Staff;
            user.IsVerified = true;
            
            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<User?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await _userRepository.GetWithRelationsAsync(
            u => u.Id == userId,
            useNoTracking: true,
            query => query.Include(u => u.UserAddresses).ThenInclude(ua => ua.Address),
            cancellationToken);
    
    public async Task<(List<User>, int totalCount)> GetAllUsersAsync(int page, int pageSize, String? role = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<User, bool>> filter = u => u.Status == UserStatus.Active;
        
        // Apply role filter
        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                filter = u => u.Role == userRole;
            }
        }
        else
        {
            // Default filter: only customers if no role specified
            filter = u => u.Role == UserRole.Customer;
        }

        return await _userRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(u => u.UpdatedAt),
            includeFunc: query => query.Include(u => u.UserAddresses).ThenInclude(ua => ua.Address),
            cancellationToken
        );
    }
    
    public async Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _userRepository.AnyAsync(u => u.Email.ToUpper() == email.ToUpper(), cancellationToken);
}