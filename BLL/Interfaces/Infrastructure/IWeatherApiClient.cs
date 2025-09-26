using BLL.DTO.Weather;

namespace BLL.Interfaces.Infrastructure;

public interface IWeatherApiClient
{
    Task<HourlyWeatherResponseDto> GetHourlyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
    Task<DailyWeatherResponseDto> GetDailyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
    Task<CurrentWeatherResponseDto> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
    Task<(decimal?[] precipitationData, decimal?[] et0Data)> GetHistoricalWeatherDataAsync(decimal latitude, decimal longitude, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}