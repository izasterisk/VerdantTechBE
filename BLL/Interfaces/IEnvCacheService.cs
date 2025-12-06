namespace BLL.Interfaces;

public interface IEnvCacheService
{
    /// <summary>
    /// Preload weather and soil data for all farms of a user (background task, no exceptions thrown)
    /// </summary>
    Task PreloadAllFarmsDataAsync(ulong userId);
}