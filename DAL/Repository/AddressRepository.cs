using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class AddressRepository : IAddressRepository
{
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<UserAddress> _userAddressRepository;
    
    public AddressRepository(IRepository<Address> addressRepository, IRepository<UserAddress> userAddressRepository)
    {
        _addressRepository = addressRepository;
        _userAddressRepository = userAddressRepository;
    }
    
    public async Task<Address> CreateAddressAsync(Address address, CancellationToken cancellationToken = default)
    {
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;
        return await _addressRepository.CreateAsync(address, cancellationToken);
    }
    
    public async Task<Address> UpdateAddressAsync(Address address, CancellationToken cancellationToken = default)
    {
        address.UpdatedAt = DateTime.UtcNow;
        return await _addressRepository.UpdateAsync(address, cancellationToken);
    }
    
    public async Task<Address?> GetAddressByIdAsync(ulong Id, CancellationToken cancellationToken = default)
    {
        return await _addressRepository.GetAsync(u =>u.Id == Id, useNoTracking: true, cancellationToken);
    }
    
    //User addresses
    public async Task<UserAddress> CreateUserAddressAsync(ulong userId, Address address, CancellationToken cancellationToken = default)
    {
        var createdAddress = await CreateAddressAsync(address, cancellationToken);
        
        var userAddress = new UserAddress
        {
            UserId = userId,
            AddressId = createdAddress.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        return await _userAddressRepository.CreateAsync(userAddress, cancellationToken);
    }
    
    public async Task<UserAddress> UpdateUserAddressAsync(UserAddress userAddress, Address address, CancellationToken cancellationToken = default)
    {
        await UpdateAddressAsync(address, cancellationToken);

        if (userAddress.IsDeleted)
        {
            userAddress.DeletedAt = DateTime.UtcNow;
        }
        userAddress.UpdatedAt = DateTime.UtcNow;
        return await _userAddressRepository.UpdateAsync(userAddress, cancellationToken);
    }
    
    public async Task<UserAddress?> GetUserAddressByAddressIdAsync(ulong Id, CancellationToken cancellationToken = default)
    {
        return await _userAddressRepository.GetAsync(ua => ua.AddressId == Id && !ua.IsDeleted, useNoTracking: true, cancellationToken: cancellationToken);
    }
}