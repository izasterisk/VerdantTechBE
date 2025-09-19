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
    
    public async Task<Address> CreateAddressWithTransactionAsync(Address address, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            address.CreatedAt = DateTime.Now;
            address.UpdatedAt = DateTime.Now;
            var createdAddress = await _addressRepository.CreateAsync(address, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdAddress;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Address> UpdateAddressWithTransactionAsync(Address address, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            address.UpdatedAt = DateTime.Now;
            var updatedAddress = await _addressRepository.UpdateAsync(address, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return updatedAddress;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<Address?> GetAddressByIdAsync(ulong Id, CancellationToken cancellationToken = default)
    {
        return await _addressRepository.GetAsync(u =>u.Id == Id, useNoTracking: true, cancellationToken);
    }
}