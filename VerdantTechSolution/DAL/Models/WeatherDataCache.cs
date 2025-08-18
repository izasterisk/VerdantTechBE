using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Cached weather data from external APIs
/// </summary>
public partial class WeatherDataCache
{
    public ulong Id { get; set; }

    public ulong FarmProfileId { get; set; }

    public string ApiSource { get; set; } = null!;

    public DateOnly WeatherDate { get; set; }

    public decimal? TemperatureMin { get; set; }

    public decimal? TemperatureMax { get; set; }

    public decimal? TemperatureAvg { get; set; }

    public decimal? HumidityPercentage { get; set; }

    public decimal? PrecipitationMm { get; set; }

    public decimal? WindSpeedKmh { get; set; }

    public string? WindDirection { get; set; }

    public decimal? UvIndex { get; set; }

    public string? WeatherCondition { get; set; }

    public string? WeatherIcon { get; set; }

    public TimeOnly? SunriseTime { get; set; }

    public TimeOnly? SunsetTime { get; set; }

    public string? RawApiResponse { get; set; }

    public DateTime? FetchedAt { get; set; }

    public virtual FarmProfile FarmProfile { get; set; } = null!;
}
