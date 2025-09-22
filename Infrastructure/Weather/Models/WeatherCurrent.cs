namespace Infrastructure.Weather.Models;

/// <summary>
/// Raw response models để deserialize JSON từ Open-Meteo API (Current Weather)
/// </summary>
public class WeatherCurrent
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Generationtime_ms { get; set; }
    public int Utc_offset_seconds { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Timezone_abbreviation { get; set; } = string.Empty;
    public decimal Elevation { get; set; }
    
    public OpenMeteoCurrentUnits Current_units { get; set; } = new();
    public OpenMeteoCurrentData Current { get; set; } = new();
}

public class OpenMeteoCurrentUnits
{
    public string Time { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
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

public class OpenMeteoCurrentData
{
    public string Time { get; set; } = string.Empty;
    public int Interval { get; set; }
    public decimal Temperature_2m { get; set; }
    public decimal Apparent_temperature { get; set; }
    public int Relative_humidity_2m { get; set; }
    public decimal Precipitation { get; set; }
    public decimal Wind_speed_10m { get; set; }
    public decimal Wind_gusts_10m { get; set; }
    public decimal Uv_index { get; set; }
    public decimal Soil_moisture_0_to_1cm { get; set; }
    public decimal Soil_moisture_3_to_9cm { get; set; }
    public decimal Soil_temperature_0cm { get; set; }
}