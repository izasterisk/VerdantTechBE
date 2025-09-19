using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAddressRepository
{
    Task<Address?> GetAddressByIdAsync(ulong Id, CancellationToken cancellationToken = default);
    Task<Address> CreateAddressWithTransactionAsync(Address address, CancellationToken cancellationToken = default);
    Task<Address> UpdateAddressWithTransactionAsync(Address address, CancellationToken cancellationToken = default);
}