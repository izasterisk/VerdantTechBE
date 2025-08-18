using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Cached weather data from external APIs
/// </summary>
public partial class WeatherDataCache
{
    public ulong Id { get; set; }

    public ulong FarmProfileId { get; set; }

    public WeatherApiSource ApiSource { get; set; }

    public DateOnly WeatherDate { get; set; }

    public decimal? TemperatureMin { get; set; }

    public decimal? TemperatureMax { get; set; }

    public decimal? TemperatureAvg { get; set; }

    public decimal? HumidityPercentage { get; set; }

    public decimal? PrecipitationMm { get; set; }

    public decimal? WindSpeedKmh { get; set; }

    [StringLength(10)]
    public string? WindDirection { get; set; }

    public decimal? UvIndex { get; set; }

    [StringLength(100)]
    public string? WeatherCondition { get; set; }

    [StringLength(50)]
    public string? WeatherIcon { get; set; }

    public TimeOnly? SunriseTime { get; set; }

    public TimeOnly? SunsetTime { get; set; }

    /// <summary>
    /// Raw API response (JSON)
    /// </summary>
    public Dictionary<string, object> RawApiResponse { get; set; } = new();

    public DateTime FetchedAt { get; set; }

    // Navigation Properties
    public virtual FarmProfile FarmProfile { get; set; } = null!;
}
