using DAL.Data.Models;

namespace DAL.IRepository;

public interface IFarmProfileRepository
{
    Task<FarmProfile?> GetFarmProfileByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<FarmProfile?> GetCoordinateByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<FarmProfile> CreateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default);
    Task<FarmProfile> UpdateFarmProfileWithTransactionAsync(FarmProfile farmProfile, Address address, CancellationToken cancellationToken = default);
    Task<FarmProfile> DeleteByChangeStatusFarmProfileAsync(ulong farmId, CancellationToken cancellationToken = default);
}