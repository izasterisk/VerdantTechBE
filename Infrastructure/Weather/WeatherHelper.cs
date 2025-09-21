using System.Globalization;

namespace Infrastructure.Weather;

/// <summary>
/// Helper class for weather-related utilities including timezone calculations and URL building
/// </summary>
public static class WeatherHelper
{
    /// <summary>
    /// Lấy ngày theo múi giờ Việt Nam (UTC+7)
    /// </summary>
    /// <param name="addDays">Số ngày cần cộng thêm (mặc định là 0 - ngày hiện tại)</param>
    /// <returns>Ngày theo format yyyy-MM-dd</returns>
    public static string GetVietnamDate(int addDays = 0)
    {
        var vietnamTime = DateTime.UtcNow.AddHours(7).AddDays(addDays);
        return vietnamTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Build URL for Open-Meteo hourly weather API
    /// </summary>
    /// <param name="baseUrl">Base API URL</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="startDate">Start date (yyyy-MM-dd)</param>
    /// <param name="endDate">End date (yyyy-MM-dd)</param>
    /// <param name="timezone">Timezone for the API</param>
    /// <returns>Complete hourly weather API URL</returns>
    public static string BuildHourlyWeatherUrl(string baseUrl, decimal latitude, decimal longitude, 
        string startDate, string endDate, string timezone)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        
        return $"{baseUrl}forecast?" +
               $"latitude={lat}&" +
               $"longitude={lon}&" +
               $"hourly=temperature_2m,apparent_temperature,relative_humidity_2m,precipitation,wind_speed_10m,wind_gusts_10m,uv_index,soil_moisture_0_to_1cm,soil_moisture_3_to_9cm,soil_temperature_0cm&" +
               $"timezone={timezone}&" +
               $"start_date={startDate}&" +
               $"end_date={endDate}";
    }

    /// <summary>
    /// Build URL for Open-Meteo daily weather API
    /// </summary>
    /// <param name="baseUrl">Base API URL</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="startDate">Start date (yyyy-MM-dd)</param>
    /// <param name="endDate">End date (yyyy-MM-dd)</param>
    /// <param name="timezone">Timezone for the API</param>
    /// <returns>Complete daily weather API URL</returns>
    public static string BuildDailyWeatherUrl(string baseUrl, decimal latitude, decimal longitude, 
        string startDate, string endDate, string timezone)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        
        return $"{baseUrl}forecast?" +
               $"latitude={lat}&" +
               $"longitude={lon}&" +
               $"daily=temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,precipitation_sum,precipitation_hours,sunshine_duration,uv_index_max,wind_speed_10m_max,wind_gusts_10m_max,et0_fao_evapotranspiration,sunrise,sunset&" +
               $"timezone={timezone}&" +
               $"start_date={startDate}&" +
               $"end_date={endDate}";
    }
}