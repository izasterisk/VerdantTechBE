using BLL.DTO.Weather;

namespace BLL.Interfaces;

public interface IWeatherService
{
    Task<HourlyWeatherResponseDto> GetHourlyWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task<DailyWeatherResponseDto> GetDailyWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task<CurrentWeatherResponseDto> GetCurrentWeatherDetailsByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
}