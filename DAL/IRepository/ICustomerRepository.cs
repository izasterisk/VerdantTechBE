using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICustomerRepository
{
    Task<User> CreateCustomerWithTransactionAsync(User customer);
    Task<User> UpdateCustomerWithTransactionAsync(User customer);
    Task<User?> GetCustomerByIdAsync(ulong userId);
    Task<(List<User> users, int totalCount)> GetAllCustomersAsync(int page, int pageSize);
    Task<bool> CheckEmailExistsAsync(string username);
}