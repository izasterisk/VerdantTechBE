namespace Infrastructure.Weather.Models;

/// <summary>
/// Raw response models để deserialize JSON từ Open-Meteo API
/// </summary>
public class WeatherHourly
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Generationtime_ms { get; set; }
    public int Utc_offset_seconds { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Timezone_abbreviation { get; set; } = string.Empty;
    public decimal Elevation { get; set; }
    
    public OpenMeteoHourlyUnits Hourly_units { get; set; } = new();
    public OpenMeteoHourlyData Hourly { get; set; } = new();
}

public class OpenMeteoHourlyUnits
{
    public string Time { get; set; } = string.Empty;
    public string Temperature_2m { get; set; } = string.Empty;
    public string Apparent_temperature { get; set; } = string.Empty;
    public string Relative_humidity_2m { get; set; } = string.Empty;
    public string Precipitation { get; set; } = string.Empty;
    public string Wind_speed_10m { get; set; } = string.Empty;
    public string Wind_gusts_10m { get; set; } = string.Empty;
    public string Uv_index { get; set; } = string.Empty;
    public string Soil_moisture_0_to_1cm { get; set; } = string.Empty;
    public string Soil_moisture_3_to_9cm { get; set; } = string.Empty;
    public string Soil_temperature_0cm { get; set; } = string.Empty;
}

public class OpenMeteoHourlyData
{
    public List<string> Time { get; set; } = new();
    public List<decimal> Temperature_2m { get; set; } = new();
    public List<decimal> Apparent_temperature { get; set; } = new();
    public List<int> Relative_humidity_2m { get; set; } = new();
    public List<decimal> Precipitation { get; set; } = new();
    public List<decimal> Wind_speed_10m { get; set; } = new();
    public List<decimal> Wind_gusts_10m { get; set; } = new();
    public List<decimal> Uv_index { get; set; } = new();
    public List<decimal> Soil_moisture_0_to_1cm { get; set; } = new();
    public List<decimal> Soil_moisture_3_to_9cm { get; set; } = new();
    public List<decimal> Soil_temperature_0cm { get; set; } = new();
}