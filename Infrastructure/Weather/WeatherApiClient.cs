using System.Globalization;
using System.Text.Json;
using BLL.DTO.Weather;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Weather.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Weather;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _archiveBaseUrl;
    private readonly string _defaultTimeZone;
    private readonly int _timeoutSeconds;

    public WeatherApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["OPEN_METEO_URL"] ?? "https://api.open-meteo.com/v1/";
        _archiveBaseUrl = _configuration["OPEN_METEO_ARCHIVE_URL"] ?? "https://archive-api.open-meteo.com/v1/";
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
            
            var rawWeatherData = JsonSerializer.Deserialize<WeatherHourly>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rawWeatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize weather data");
            }
            
            var transformedData = WeatherResponseTransformer.TransformHourlyResponse(rawWeatherData);
            return transformedData;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Dữ liệu thời tiết không hợp lệ.");
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
            
            var rawWeatherData = JsonSerializer.Deserialize<WeatherDaily>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rawWeatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize weather data");
            }
            
            var transformedData = WeatherResponseTransformer.TransformDailyResponse(rawWeatherData);
            return transformedData;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Dữ liệu thời tiết không hợp lệ.");
        }
    }

    public async Task<CurrentWeatherResponseDto> GetCurrentWeatherAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = WeatherHelper.BuildCurrentWeatherUrl(_baseUrl, latitude, longitude, _defaultTimeZone);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var rawWeatherData = JsonSerializer.Deserialize<WeatherCurrent>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rawWeatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize weather data");
            }
            
            var transformedData = WeatherResponseTransformer.TransformCurrentResponse(rawWeatherData);
            return transformedData;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Dữ liệu thời tiết không hợp lệ.");
        }
    }

    public async Task<Dictionary<DateOnly, (decimal? precipitationSum, decimal? et0FaoEvapotranspiration)>> GetHistoricalWeatherDataAsync(decimal latitude, decimal longitude, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            WeatherHelper.ValidateDateRange(startDate, endDate);
            
            var startDateStr = startDate.ToString("yyyy-MM-dd");
            var endDateStr = endDate.ToString("yyyy-MM-dd");
            var url = WeatherHelper.BuildHistoricalWeatherUrl(_archiveBaseUrl, latitude, longitude, startDateStr, endDateStr, _defaultTimeZone);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var rawWeatherData = JsonSerializer.Deserialize<WeatherHistorical>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rawWeatherData == null)
            {
                throw new InvalidOperationException("Failed to deserialize historical weather data");
            }
            
            var result = new Dictionary<DateOnly, (decimal?, decimal?)>();
            var dates = rawWeatherData.Daily?.Time;
            var precipitationData = rawWeatherData.Daily?.Precipitation_sum;
            var et0Data = rawWeatherData.Daily?.Et0_fao_evapotranspiration;
            
            if (dates != null)
            {
                for (int i = 0; i < dates.Count; i++)
                {
                    var date = DateOnly.Parse(dates[i]);
                    var precipitation = precipitationData != null && i < precipitationData.Count ? precipitationData[i] : null;
                    var et0 = et0Data != null && i < et0Data.Count ? et0Data[i] : null;
                    if (precipitation.HasValue && et0.HasValue)
                    {
                        result[date] = (precipitation, et0);
                    }
                }
            }
            return result;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server thời tiết hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Dữ liệu thời tiết không hợp lệ.");
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation errors as-is
        }
    }
}