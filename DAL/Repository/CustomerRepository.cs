using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly IRepository<User> _userRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public CustomerRepository(VerdantTechDbContext context)
    {
        _userRepository = new Repository<User>(context);
        _dbContext = context;
    }
    
    public async Task<User> CreateCustomerWithTransactionAsync(User customer)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            customer.LastLoginAt = DateTime.Now;
            customer.CreatedAt = DateTime.Now;
            customer.UpdatedAt = DateTime.Now;
            var createdCustomer = await _userRepository.CreateAsync(customer);
            await transaction.CommitAsync();
            return createdCustomer;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> UpdateCustomerWithTransactionAsync(User customer)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var updatedCustomer = await _userRepository.UpdateAsync(customer);
            await transaction.CommitAsync();
            return updatedCustomer;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<User?> GetCustomerByIdAsync(ulong userId)
    {
        return await _userRepository.GetAsync(u =>u.Id == userId && u.Role == UserRole.Customer && u.Status == UserStatus.Active, useNoTracking: true);
    }
    
    public async Task<(List<User> users, int totalCount)> GetAllCustomersAsync(int page, int pageSize)
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