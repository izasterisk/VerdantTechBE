using DAL.Data.Models;

namespace DAL.IRepository;

public interface IFarmProfileRepository
{
    Task<FarmProfile> GetFarmProfileWithRelationByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<FarmProfile?> GetCoordinateByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<FarmProfile> CreateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, List<Crop> crops, CancellationToken cancellationToken = default);
    Task<FarmProfile> UpdateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default);
    Task<FarmProfile> GetFarmProfileByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    
}