using BLL.Interfaces;
using AutoMapper;
using BLL.DTO;
using BLL.DTO.Customer;
using BLL.Utils;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly IMapper _mapper;
    private readonly ICustomerRepository _customerRepository;
    
    public CustomerService(IMapper mapper, ICustomerRepository customerRepository)
    {
        _mapper = mapper;
        _customerRepository = customerRepository;
    }

    public async Task<CustomerReadOnlyDTO> CreateCustomerAsync(CustomerCreateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var emailExists = await _customerRepository.CheckEmailExistsAsync(dto.Email);
        if (emailExists)
        {
            throw new Exception($"Email {dto.Email} already exists.");
        }
        
        User customer = _mapper.Map<User>(dto);
        customer.Role = UserRole.Customer;
        customer.IsVerified = false;
        customer.Status = UserStatus.Active;
        customer.LastLoginAt = DateTime.Now;
        customer.CreatedAt = DateTime.Now;
        customer.UpdatedAt = DateTime.Now;
        customer.PasswordHash = AuthUtils.HashPassword(dto.Password);
        
        var createdCustomer = await _customerRepository.CreateCustomerWithTransactionAsync(customer);
        return _mapper.Map<CustomerReadOnlyDTO>(createdCustomer);
    }

    public async Task<CustomerReadOnlyDTO?> GetCustomerByIdAsync(ulong customerId)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
        return customer == null ? null : _mapper.Map<CustomerReadOnlyDTO>(customer);
    }

    public async Task<PagedResponse<CustomerReadOnlyDTO>> GetAllCustomersAsync(int page, int pageSize)
    {
        var (customers, totalCount) = await _customerRepository.GetAllCustomersAsync(page, pageSize);
        var customerDtos = _mapper.Map<List<CustomerReadOnlyDTO>>(customers);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<CustomerReadOnlyDTO>
        {
            Data = customerDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<CustomerReadOnlyDTO> UpdateCustomerAsync(ulong customerId, CustomerDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} is null");
        
        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(customerId);
        if (existingCustomer == null)
        {
            throw new Exception($"Customer with ID {customerId} not found.");
        }

        User customer = _mapper.Map<User>(dto);

        var updatedCustomer = await _customerRepository.UpdateCustomerWithTransactionAsync(existingCustomer);
        return _mapper.Map<CustomerReadOnlyDTO>(updatedCustomer);
    }
}