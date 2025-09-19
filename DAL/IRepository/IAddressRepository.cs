using DAL.Data.Models;

namespace DAL.IRepository;

public interface IAddressRepository
{
    Task<Address?> GetAddressByIdAsync(ulong Id, CancellationToken cancellationToken = default);
    Task<Address> CreateAsync(Address address, CancellationToken cancellationToken = default);
    Task<Address> UpdateAsync(Address address, CancellationToken cancellationToken = default);
}