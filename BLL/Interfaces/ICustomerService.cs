using BLL.DTO;
using BLL.DTO.Customer;

namespace BLL.Interfaces;

public interface ICustomerService
{
    Task<CustomerReadOnlyDTO> CreateCustomerAsync(CustomerCreateDTO dto);
    Task<CustomerReadOnlyDTO?> GetCustomerByIdAsync(ulong customerId);
    Task<PagedResponse<CustomerReadOnlyDTO>> GetAllCustomersAsync(int page, int pageSize);
    Task<CustomerReadOnlyDTO> UpdateCustomerAsync(ulong customerId, CustomerDTO dto);
}