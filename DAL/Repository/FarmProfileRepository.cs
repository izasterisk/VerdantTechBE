using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class FarmProfileRepository : IFarmProfileRepository
{
    private readonly IRepository<FarmProfile> _farmProfileRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IAddressRepository _addressRepository;
    
    public FarmProfileRepository(IRepository<FarmProfile> farmProfileRepository, VerdantTechDbContext dbContext, IAddressRepository addressRepository)
    {
        _farmProfileRepository = farmProfileRepository;
        _dbContext = dbContext;
        _addressRepository = addressRepository;
    }

    public async Task<FarmProfile?> GetFarmProfileByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default)
    {
        return await _farmProfileRepository.GetWithRelationsAsync(
            f => f.Id == farmId,
            useNoTracking,
            query => query.Include(f => f.User).Include(f => f.Address),
            cancellationToken);
    }

    public async Task<FarmProfile?> GetCoordinateByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default)
    {
        return await _farmProfileRepository.GetWithRelationsAsync(
            f => f.Id == farmId,
            useNoTracking,
            query => query.Include(f => f.Address),
            cancellationToken);
    }
    
    public async Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default)
    {
        return await _farmProfileRepository.GetAllWithRelationsByFilterAsync(
            f => f.UserId == userId,
            useNoTracking,
            query => query.Include(f => f.User).Include(f => f.Address),
            cancellationToken);
    }
    
    public async Task<FarmProfile> CreateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            address.CreatedAt = DateTime.UtcNow;
            address.UpdatedAt = DateTime.UtcNow;
            var createdAddress = await _addressRepository.CreateAddressAsync(address, cancellationToken);
            
            farmProfile.AddressId = createdAddress.Id;
            farmProfile.CreatedAt = DateTime.UtcNow;
            farmProfile.UpdatedAt = DateTime.UtcNow;
            farmProfile.Status = FarmProfileStatus.Active;
            
            var createdFarmProfile = await _farmProfileRepository.CreateAsync(farmProfile, cancellationToken);
            
            var farmProfileWithRelations = await _farmProfileRepository.GetWithRelationsAsync(
                f => f.Id == createdFarmProfile.Id,
                useNoTracking: true,
                query => query.Include(f => f.User).Include(f => f.Address),
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return farmProfileWithRelations ?? createdFarmProfile;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<FarmProfile> UpdateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            address.UpdatedAt = DateTime.UtcNow;
            await _addressRepository.UpdateAddressAsync(address, cancellationToken);
            
            farmProfile.UpdatedAt = DateTime.UtcNow;
            var updatedFarmProfile = await _farmProfileRepository.UpdateAsync(farmProfile, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            var farmProfileWithRelations = await _farmProfileRepository.GetWithRelationsAsync(
                f => f.Id == updatedFarmProfile.Id,
                useNoTracking: true,
                query => query.Include(f => f.User).Include(f => f.Address),
                cancellationToken);
            return farmProfileWithRelations ?? updatedFarmProfile;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
