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
            f => f.Id == farmId && f.Status != FarmProfileStatus.Deleted,
            useNoTracking,
            query => query.Include(f => f.User).Include(f => f.Address),
            cancellationToken);
    }

    public async Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default)
    {
        return await _farmProfileRepository.GetAllWithRelationsByFilterAsync(
            f => f.UserId == userId && f.Status != FarmProfileStatus.Deleted,
            useNoTracking,
            query => query.Include(f => f.User).Include(f => f.Address),
            cancellationToken);
    }
    
    public async Task<FarmProfile> CreateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            address.CreatedAt = DateTime.Now;
            address.UpdatedAt = DateTime.Now;
            var createdAddress = await _addressRepository.CreateAsync(address, cancellationToken);
            
            farmProfile.AddressId = createdAddress.Id;
            farmProfile.CreatedAt = DateTime.Now;
            farmProfile.UpdatedAt = DateTime.Now;
            farmProfile.Status = FarmProfileStatus.Active;
            
            var createdFarmProfile = await _farmProfileRepository.CreateAsync(farmProfile, cancellationToken);
            
            var farmProfileWithRelations = await _dbContext.FarmProfiles
                .Include(f => f.User)
                .Include(f => f.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == createdFarmProfile.Id, cancellationToken);
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
            address.UpdatedAt = DateTime.Now;
            await _addressRepository.UpdateAsync(address, cancellationToken);
            
            farmProfile.UpdatedAt = DateTime.Now;
            var updatedFarmProfile = await _farmProfileRepository.UpdateAsync(farmProfile, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            var farmProfileWithRelations = await _dbContext.FarmProfiles
                .Include(f => f.User)
                .Include(f => f.Address)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == updatedFarmProfile.Id, cancellationToken);
            return farmProfileWithRelations ?? updatedFarmProfile;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
