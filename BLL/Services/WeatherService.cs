using BLL.DTO.Weather;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class WeatherService : IWeatherService
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly IMemoryCache _cache;

    public WeatherService(IFarmProfileRepository farmProfileRepository, IWeatherApiClient weatherApiClient, IMemoryCache cache)
    {
        _farmProfileRepository = farmProfileRepository;
        _weatherApiClient = weatherApiClient;
        _cache = cache;
    }

    public async Task<HourlyWeatherResponseDto> GetHourlyWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"weather:hourly:{farmId}";
        if (_cache.TryGetValue(cacheKey, out object? cachedObj) && 
            cachedObj?.GetType().GetProperty("Data")?.GetValue(cachedObj) is HourlyWeatherResponseDto cachedData)
        {
            return cachedData;
        }
        
        // Cache miss - get from API
        var farmProfile = await _farmProfileRepository.GetFarmWithAddressByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile.Address?.Latitude == null || farmProfile.Address.Longitude == null)
        {
            throw new InvalidOperationException("Nông trại của bạn chưa có địa chỉ, không thể tìm được thông tin thời tiết tương ứng!");
        }
        try
        {
            return await _weatherApiClient.GetHourlyWeatherAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
    }

    public async Task<DailyWeatherResponseDto> GetDailyWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"weather:daily:{farmId}";
        if (_cache.TryGetValue(cacheKey, out object? cachedObj) && 
            cachedObj?.GetType().GetProperty("Data")?.GetValue(cachedObj) is DailyWeatherResponseDto cachedData)
        {
            return cachedData;
        }
        
        // Cache miss - get from API
        var farmProfile = await _farmProfileRepository.GetFarmWithAddressByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile.Address?.Latitude == null || farmProfile.Address.Longitude == null)
        {
            throw new InvalidOperationException("Nông trại của bạn chưa có địa chỉ, không thể tìm được thông tin thời tiết tương ứng!");
        }
        try
        {
            return await _weatherApiClient.GetDailyWeatherAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
    }

    public async Task<CurrentWeatherResponseDto> GetCurrentWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cacheKey = $"weather:current:{farmId}";
        if (_cache.TryGetValue(cacheKey, out object? cachedObj) && 
            cachedObj?.GetType().GetProperty("Data")?.GetValue(cachedObj) is CurrentWeatherResponseDto cachedData)
        {
            return cachedData;
        }
        
        // Cache miss - get from API
        var farmProfile = await _farmProfileRepository.GetFarmWithAddressByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile.Address?.Latitude == null || farmProfile.Address.Longitude == null)
        {
            throw new InvalidOperationException("Nông trại của bạn chưa có địa chỉ, không thể tìm được thông tin thời tiết tương ứng!");
        }
        try
        {
            return await _weatherApiClient.GetCurrentWeatherAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
    }
}