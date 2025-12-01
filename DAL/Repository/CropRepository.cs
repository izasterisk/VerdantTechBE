using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class CropRepository : ICropRepository
{
    private readonly IRepository<FarmProfile> _farmProfileRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<Crop> _cropRepository;
    
    public CropRepository(IRepository<FarmProfile> farmProfileRepository, VerdantTechDbContext dbContext,
        IRepository<Crop> cropRepository)
    {
        _farmProfileRepository = farmProfileRepository;
        _dbContext = dbContext;
        _cropRepository = cropRepository;
    }
    
    public async Task<Crop> GetCropBelongToFarm(ulong cropId, ulong farmId, CancellationToken cancellationToken = default)
    {
        return await _cropRepository.GetAsync(c => c.Id == cropId && c.FarmProfileId == farmId, true, cancellationToken)
               ?? throw new KeyNotFoundException("Cây trồng không thuộc về trang trại hoặc không tồn tại");
    }
    
    public async Task<bool> IsFarmExistsAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        var x = await _farmProfileRepository.AnyAsync(f => f.Id == farmId
            && f.Status != FarmProfileStatus.Deleted, cancellationToken);
        if (!x)
            throw new KeyNotFoundException($"Không tìm thấy hồ sơ trang trại với ID: {farmId} hoặc nó đã bị xóa.");
        return x;
    }
    
    public async Task<List<Crop>> GetAllPlantingCropsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        return await _cropRepository.GetAllByFilterAsync(c => c.FarmProfileId == farmId && c.Status != CropStatus.Completed
            && c.Status != CropStatus.Deleted && c.Status != CropStatus.Failed, true, cancellationToken);
    }
    
    public async Task<List<Crop>> GetAllCropsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        return await _cropRepository.GetAllByFilterAsync(c => c.FarmProfileId == farmId, true, cancellationToken);
    }
    
    public async Task CreateBulkCropsAsync(List<Crop> crops, CancellationToken cancellationToken = default)
    {
        await _dbContext.Crops.AddRangeAsync(crops, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBulkCropsAsync(List<Crop> crops, CancellationToken cancellationToken = default)
    {
        await _cropRepository.BulkUpdateAsync(crops, cancellationToken);
    }
}