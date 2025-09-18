using DAL.Data.Models;

namespace DAL.IRepository;

public interface IFarmProfileRepository
{
    Task<FarmProfile?> GetFarmProfileByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default);

    Task<FarmProfile> CreateAsync(FarmProfile entity, CancellationToken cancellationToken = default);
    Task<FarmProfile> UpdateAsync(FarmProfile entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(FarmProfile entity, CancellationToken cancellationToken = default);
}