using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAddressRepository
{
    Task<Address?> GetAddressByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<Address> CreateAddressAsync(Address address, CancellationToken cancellationToken = default);
    Task<Address> UpdateAddressAsync(Address address, CancellationToken cancellationToken = default);

    Task<UserAddress> CreateUserAddressAsync(ulong userId, Address address, CancellationToken cancellationToken = default);
    Task<UserAddress> UpdateUserAddressAsync(UserAddress userAddress, Address address, CancellationToken cancellationToken = default);
    Task<UserAddress?> GetUserAddressByAddressIdAsync(ulong Id, CancellationToken cancellationToken = default);
}