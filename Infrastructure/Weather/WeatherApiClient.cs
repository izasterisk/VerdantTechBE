using System.Globalization;
using System.Text.Json;
using BLL.DTO.Weather;
using BLL.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Weather;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _defaultTimeZone;
    private readonly int _timeoutSeconds;

    public WeatherApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["OPEN_METEO_URL"] ?? "https://api.open-meteo.com/v1/";
        _defaultTimeZone = _configuration["DEFAULT_TIME_ZONE"] ?? "Asia/Ho_Chi_Minh";
        _timeoutSeconds = int.Parse(_configuration["TIME_OUT_SECONDS"] ?? "10");
        
        // Configure HttpClient timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    }

    public async Task<HourlyWeatherResponseDto> GetHourlyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = WeatherHelper.GetVietnamDate();
            var url = WeatherHelper.BuildHourlyWeatherUrl(_baseUrl, latitude, longitude, today, today, _defaultTimeZone);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherData = JsonSerializer.Deserialize<HourlyWeatherResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return weatherData ?? throw new InvalidOperationException("Failed to deserialize weather data");
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
    }

    public async Task<DailyWeatherResponseDto> GetDailyWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = WeatherHelper.GetVietnamDate();
            var endDate = WeatherHelper.GetVietnamDate(6); // 6 ngày sau hôm nay
            var url = WeatherHelper.BuildDailyWeatherUrl(_baseUrl, latitude, longitude, today, endDate, _defaultTimeZone);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherData = JsonSerializer.Deserialize<DailyWeatherResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return weatherData ?? throw new InvalidOperationException("Failed to deserialize weather data");
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
    }
}