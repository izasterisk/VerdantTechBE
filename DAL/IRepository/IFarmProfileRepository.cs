using DAL.Data.Models;

namespace DAL.IRepository;

public interface IFarmProfileRepository
{
    Task<FarmProfile?> GetFarmProfileByFarmIdAsync(ulong farmId, bool useNoTracking = true);
    Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true);

    Task<FarmProfile> CreateAsync(FarmProfile entity);
    Task<FarmProfile> UpdateAsync(FarmProfile entity);
    Task<bool> DeleteAsync(FarmProfile entity);
}