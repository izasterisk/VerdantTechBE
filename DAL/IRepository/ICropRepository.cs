using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICropRepository
{
    Task<bool> IsFarmExistsAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task<List<Crop>> GetAllPlantingCropsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task CreateBulkCropsAsync(List<Crop> crops, CancellationToken cancellationToken = default);
    Task UpdateBulkCropsAsync(List<Crop> crops, CancellationToken cancellationToken = default);
    Task<List<Crop>> GetAllCropsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
}