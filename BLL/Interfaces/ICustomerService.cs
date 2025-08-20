using BLL.DTO.Customer;

namespace BLL.Interfaces;

public interface ICustomerService
{
    Task<CustomerReadOnlyDTO> CreateCustomerAsync(CustomerCreateDTO dto);
    Task<CustomerReadOnlyDTO?> GetCustomerByIdAsync(ulong customerId);
    Task<List<CustomerReadOnlyDTO>> GetAllCustomersAsync();
    Task<CustomerReadOnlyDTO> UpdateCustomerAsync(ulong customerId, CustomerDTO dto);
}