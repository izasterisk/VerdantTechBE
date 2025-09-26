namespace Infrastructure.Weather.Models;

/// <summary>
/// Raw response model for OpenMeteo Archive API (ERA5 historical data)
/// </summary>
public class WeatherHistorical
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Generationtime_ms { get; set; }
    public int Utc_offset_seconds { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string Timezone_abbreviation { get; set; } = string.Empty;
    public decimal Elevation { get; set; }
    
    public OpenMeteoHistoricalUnits Daily_units { get; set; } = new();
    public OpenMeteoHistoricalData Daily { get; set; } = new();
}

public class OpenMeteoHistoricalUnits
{
    public string Time { get; set; } = string.Empty;
    public string Precipitation_sum { get; set; } = string.Empty;
    public string Et0_fao_evapotranspiration { get; set; } = string.Empty;
}

public class OpenMeteoHistoricalData
{
    public List<string> Time { get; set; } = new();
    public List<decimal?> Precipitation_sum { get; set; } = new();
    public List<decimal?> Et0_fao_evapotranspiration { get; set; } = new();
}