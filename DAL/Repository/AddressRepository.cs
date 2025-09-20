using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class AddressRepository : IAddressRepository
{
    private readonly IRepository<Address> _addressRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public AddressRepository(VerdantTechDbContext context)
    {
        _addressRepository = new Repository<Address>(context);
        _dbContext = context;
    }
    
    public async Task<Address> CreateAsync(Address address, CancellationToken cancellationToken = default)
    {
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;
        return await _addressRepository.CreateAsync(address, cancellationToken);
    }
    
    public async Task<Address> UpdateAsync(Address address, CancellationToken cancellationToken = default)
    {
        address.UpdatedAt = DateTime.UtcNow;
        return await _addressRepository.UpdateAsync(address, cancellationToken);
    }
    
    public async Task<Address?> GetAddressByIdAsync(ulong Id, CancellationToken cancellationToken = default)
    {
        return await _addressRepository.GetAsync(u =>u.Id == Id, useNoTracking: true, cancellationToken);
    }
}