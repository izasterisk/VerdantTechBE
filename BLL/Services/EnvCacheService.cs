using BLL.DTO.Soil;
using BLL.DTO.Weather;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

/// <summary>
/// Wrapper class to store cached data with timestamp
/// </summary>
public class CachedData<T>
{
    public T Data { get; set; } = default!;
    public DateTime CachedAt { get; set; }
}

public class EnvCacheService : IEnvCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ISoilGridsApiClient _soilGridsApiClient;
    private readonly IFarmProfileRepository _farmProfileRepository;
    
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);
    
    public EnvCacheService(
        IMemoryCache cache,
        IWeatherApiClient weatherApiClient,
        ISoilGridsApiClient soilGridsApiClient,
        IFarmProfileRepository farmProfileRepository)
    {
        _cache = cache;
        _weatherApiClient = weatherApiClient;
        _soilGridsApiClient = soilGridsApiClient;
        _farmProfileRepository = farmProfileRepository;
    }
    
    public async Task PreloadAllFarmsDataAsync(ulong userId)
    {
        var farms = await _farmProfileRepository.GetAllFarmWithAddressByUserIdAsync(userId, false);
        if (farms.Count == 0)
            return;
        foreach (var farm in farms)
        {
            if (farm.Address?.Latitude == null || farm.Address.Longitude == null)
                continue;
            
            if (ShouldSkipPreload(farm.Id))
                continue;
            
            var lat = farm.Address.Latitude.Value;
            var lon = farm.Address.Longitude.Value;
            
            var weatherTask = PreloadWeatherSequentialAsync(farm.Id, lat, lon);
            var soilTask = PreloadSoilWithRetryAsync(farm.Id, lat, lon);
            
            await Task.WhenAll(weatherTask, soilTask);
            
            if (farm.Id != farms.Last().Id)
                await Task.Delay(1000);
        }
    }
    
    private bool ShouldSkipPreload(ulong farmId)
    {
        // Check if all 5 APIs have fresh cache (< 30 minutes old)
        return IsCacheFresh($"weather:current:{farmId}") &&
               IsCacheFresh($"weather:daily:{farmId}") &&
               IsCacheFresh($"weather:hourly:{farmId}") &&
               IsCacheFresh($"weather:historical:{farmId}") &&
               IsCacheFresh($"soil:{farmId}");
    }
    
    private bool IsCacheFresh(string cacheKey)
    {
        if (!_cache.TryGetValue(cacheKey, out object? cachedObj))
            return false;
        
        // Extract CachedAt from wrapper
        var cachedAtProperty = cachedObj?.GetType().GetProperty("CachedAt");
        if (cachedAtProperty == null)
            return false;
        
        var cachedAt = (DateTime)(cachedAtProperty.GetValue(cachedObj) ?? DateTime.MinValue);
        var age = DateTime.UtcNow - cachedAt;
        
        return age.TotalMinutes < 30;
    }
    
    private async Task PreloadWeatherSequentialAsync(ulong farmId, decimal lat, decimal lon)
    {
        // Current Weather
        await PreloadCurrentWeatherWithRetryAsync(farmId, lat, lon);
        await Task.Delay(1000);
        
        // Daily Weather
        await PreloadDailyWeatherWithRetryAsync(farmId, lat, lon);
        await Task.Delay(1000);
        
        // Hourly Weather
        await PreloadHourlyWeatherWithRetryAsync(farmId, lat, lon);
        await Task.Delay(1000);
        
        // Historical Weather (1 year)
        await PreloadHistoricalWeatherWithRetryAsync(farmId, lat, lon);
    }
    
    private async Task PreloadCurrentWeatherWithRetryAsync(ulong farmId, decimal lat, decimal lon)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var data = await _weatherApiClient.GetCurrentWeatherAsync(lat, lon);
                var wrapper = new CachedData<CurrentWeatherResponseDto>
                {
                    Data = data,
                    CachedAt = DateTime.UtcNow
                };
                _cache.Set($"weather:current:{farmId}", wrapper, _cacheDuration);
                break;
            }
            catch
            {
                if (i < 9)
                    await Task.Delay(1000);
            }
        }
    }
    
    private async Task PreloadDailyWeatherWithRetryAsync(ulong farmId, decimal lat, decimal lon)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var data = await _weatherApiClient.GetDailyWeatherAsync(lat, lon);
                var wrapper = new CachedData<DailyWeatherResponseDto>
                {
                    Data = data,
                    CachedAt = DateTime.UtcNow
                };
                _cache.Set($"weather:daily:{farmId}", wrapper, _cacheDuration);
                break;
            }
            catch
            {
                if (i < 9)
                    await Task.Delay(1000);
            }
        }
    }
    
    private async Task PreloadHourlyWeatherWithRetryAsync(ulong farmId, decimal lat, decimal lon)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var data = await _weatherApiClient.GetHourlyWeatherAsync(lat, lon);
                var wrapper = new CachedData<HourlyWeatherResponseDto>
                {
                    Data = data,
                    CachedAt = DateTime.UtcNow
                };
                _cache.Set($"weather:hourly:{farmId}", wrapper, _cacheDuration);
                break;
            }
            catch
            {
                if (i < 9)
                    await Task.Delay(1000);
            }
        }
    }
    
    private async Task PreloadHistoricalWeatherWithRetryAsync(ulong farmId, decimal lat, decimal lon)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var startDate = endDate.AddYears(-1);
                
                var data = await _weatherApiClient.GetHistoricalWeatherDataAsync(lat, lon, startDate, endDate);
                var wrapper = new CachedData<Dictionary<DateOnly, (decimal?, decimal?)>>
                {
                    Data = data,
                    CachedAt = DateTime.UtcNow
                };
                _cache.Set($"weather:historical:{farmId}", wrapper, _cacheDuration);
                break;
            }
            catch
            {
                if (i < 9)
                    await Task.Delay(1000);
            }
        }
    }
    
    private async Task PreloadSoilWithRetryAsync(ulong farmId, decimal lat, decimal lon)
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var data = await _soilGridsApiClient.GetSoilDataAsync(lat, lon);
                var wrapper = new CachedData<SoilDataResult>
                {
                    Data = data,
                    CachedAt = DateTime.UtcNow
                };
                _cache.Set($"soil:{farmId}", wrapper, _cacheDuration);
                break;
            }
            catch
            {
                if (i < 9)
                    await Task.Delay(1000);
            }
        }
    }
}