using BLL.DTO.Weather;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;

namespace BLL.Services;

public class WeatherService : IWeatherService
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly IWeatherApiClient _weatherApiClient;

    public WeatherService(IFarmProfileRepository farmProfileRepository, IWeatherApiClient weatherApiClient)
    {
        _farmProfileRepository = farmProfileRepository;
        _weatherApiClient = weatherApiClient;
    }

    public async Task<HourlyWeatherResponseDto> GetHourlyWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        // Get farm profile with address coordinates
        var farmProfile = await _farmProfileRepository.GetCoordinateByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile?.Address?.Latitude == null || farmProfile.Address.Longitude == null)
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
        // Get farm profile with address coordinates
        var farmProfile = await _farmProfileRepository.GetCoordinateByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile?.Address?.Latitude == null || farmProfile.Address.Longitude == null)
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
        // Get farm profile with address coordinates
        var farmProfile = await _farmProfileRepository.GetCoordinateByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile?.Address?.Latitude == null || farmProfile.Address.Longitude == null)
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