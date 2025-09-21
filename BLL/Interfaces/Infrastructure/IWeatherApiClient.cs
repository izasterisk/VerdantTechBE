using BLL.DTO.Weather;

namespace BLL.Interfaces.Infrastructure;

public interface IWeatherApiClient
{
    Task<HourlyWeatherResponseDto> GetHourlyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
    Task<DailyWeatherResponseDto> GetDailyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
}