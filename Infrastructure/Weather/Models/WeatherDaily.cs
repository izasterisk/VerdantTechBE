namespace Infrastructure.Weather.Models;

/// <summary>
/// Raw response models để deserialize JSON từ Open-Meteo API (Daily)
/// </summary>
public class WeatherDaily
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Generationtime_ms { get; set; }
    public int Utc_offset_seconds { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Timezone_abbreviation { get; set; } = string.Empty;
    public decimal Elevation { get; set; }
    
    public OpenMeteoDailyUnits Daily_units { get; set; } = new();
    public OpenMeteoDailyData Daily { get; set; } = new();
}

public class OpenMeteoDailyUnits
{
    public string Time { get; set; } = string.Empty;
    public string Temperature_2m_max { get; set; } = string.Empty;
    public string Temperature_2m_min { get; set; } = string.Empty;
    public string Apparent_temperature_max { get; set; } = string.Empty;
    public string Apparent_temperature_min { get; set; } = string.Empty;
    public string Precipitation_sum { get; set; } = string.Empty;
    public string Precipitation_hours { get; set; } = string.Empty;
    public string Sunshine_duration { get; set; } = string.Empty;
    public string Uv_index_max { get; set; } = string.Empty;
    public string Wind_speed_10m_max { get; set; } = string.Empty;
    public string Wind_gusts_10m_max { get; set; } = string.Empty;
    public string Et0_fao_evapotranspiration { get; set; } = string.Empty;
    public string Sunrise { get; set; } = string.Empty;
    public string Sunset { get; set; } = string.Empty;
}

public class OpenMeteoDailyData
{
    public List<string> Time { get; set; } = new();
    public List<decimal> Temperature_2m_max { get; set; } = new();
    public List<decimal> Temperature_2m_min { get; set; } = new();
    public List<decimal> Apparent_temperature_max { get; set; } = new();
    public List<decimal> Apparent_temperature_min { get; set; } = new();
    public List<decimal> Precipitation_sum { get; set; } = new();
    public List<decimal> Precipitation_hours { get; set; } = new();
    public List<decimal> Sunshine_duration { get; set; } = new();
    public List<decimal> Uv_index_max { get; set; } = new();
    public List<decimal> Wind_speed_10m_max { get; set; } = new();
    public List<decimal> Wind_gusts_10m_max { get; set; } = new();
    public List<decimal> Et0_fao_evapotranspiration { get; set; } = new();
    public List<string> Sunrise { get; set; } = new();
    public List<string> Sunset { get; set; } = new();
}