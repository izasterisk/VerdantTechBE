using DAL.Data.Models;

namespace BLL.Interfaces;

public interface IEnvCacheService
{
    /// <summary>
    /// Preload weather and soil data for all farms (background task, no exceptions thrown)
    /// </summary>
    Task PreloadAllFarmsDataAsync(List<FarmProfile> farms);
}