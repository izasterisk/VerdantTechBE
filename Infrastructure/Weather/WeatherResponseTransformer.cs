using BLL.DTO.Weather;
using Infrastructure.Weather.Models;
using System.Globalization;

namespace Infrastructure.Weather;

/// <summary>
/// Transformer để convert raw Open-Meteo API response thành custom DTOs
/// </summary>
public static class WeatherResponseTransformer
{
    /// <summary>
    /// Transform raw hourly response từ API thành custom HourlyWeatherResponseDto
    /// </summary>
    public static HourlyWeatherResponseDto TransformHourlyResponse(WeatherHourly raw)
    {
        var result = new HourlyWeatherResponseDto
        {
            Latitude = raw.Latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = raw.Longitude.ToString(CultureInfo.InvariantCulture),
            GenerationTimeMs = raw.Generationtime_ms.ToString(CultureInfo.InvariantCulture),
            UtcOffsetSeconds = raw.Utc_offset_seconds.ToString(),
            Timezone = raw.Timezone,
            TimezoneAbbreviation = raw.Timezone_abbreviation,
            Elevation = raw.Elevation.ToString(CultureInfo.InvariantCulture),
            
            Hourly_Units = new HourlyUnitsDto
            {
                Time = raw.Hourly_units.Time,
                Temperature_2m = raw.Hourly_units.Temperature_2m,
                Apparent_Temperature = raw.Hourly_units.Apparent_temperature,
                Relative_Humidity_2m = raw.Hourly_units.Relative_humidity_2m,
                Precipitation = raw.Hourly_units.Precipitation,
                Wind_Speed_10m = raw.Hourly_units.Wind_speed_10m,
                Wind_Gusts_10m = raw.Hourly_units.Wind_gusts_10m,
                Uv_Index = raw.Hourly_units.Uv_index,
                Soil_Moisture_0_to_1cm = raw.Hourly_units.Soil_moisture_0_to_1cm,
                Soil_Moisture_3_to_9cm = raw.Hourly_units.Soil_moisture_3_to_9cm,
                Soil_Temperature_0cm = raw.Hourly_units.Soil_temperature_0cm
            },
            
            Hourly = new List<HourlyDataDto>()
        };

        // Transform hourly data arrays thành list of objects
        for (int i = 0; i < raw.Hourly.Time.Count; i++)
        {
            result.Hourly.Add(new HourlyDataDto
            {
                Time = raw.Hourly.Time[i],
                Temperature_2m = raw.Hourly.Temperature_2m[i].ToString(CultureInfo.InvariantCulture),
                Apparent_Temperature = raw.Hourly.Apparent_temperature[i].ToString(CultureInfo.InvariantCulture),
                Relative_Humidity_2m = raw.Hourly.Relative_humidity_2m[i].ToString(),
                Precipitation = raw.Hourly.Precipitation[i].ToString(CultureInfo.InvariantCulture),
                Wind_Speed_10m = raw.Hourly.Wind_speed_10m[i].ToString(CultureInfo.InvariantCulture),
                Wind_Gusts_10m = raw.Hourly.Wind_gusts_10m[i].ToString(CultureInfo.InvariantCulture),
                Uv_Index = raw.Hourly.Uv_index[i].ToString(CultureInfo.InvariantCulture),
                Soil_Moisture_0_to_1cm = raw.Hourly.Soil_moisture_0_to_1cm[i].ToString(CultureInfo.InvariantCulture),
                Soil_Moisture_3_to_9cm = raw.Hourly.Soil_moisture_3_to_9cm[i].ToString(CultureInfo.InvariantCulture),
                Soil_Temperature_0cm = raw.Hourly.Soil_temperature_0cm[i].ToString(CultureInfo.InvariantCulture)
            });
        }

        return result;
    }

    /// <summary>
    /// Transform raw daily response từ API thành custom DailyWeatherResponseDto
    /// </summary>
    public static DailyWeatherResponseDto TransformDailyResponse(WeatherDaily raw)
    {
        var result = new DailyWeatherResponseDto
        {
            Latitude = raw.Latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = raw.Longitude.ToString(CultureInfo.InvariantCulture),
            GenerationTimeMs = raw.Generationtime_ms.ToString(CultureInfo.InvariantCulture),
            UtcOffsetSeconds = raw.Utc_offset_seconds.ToString(),
            Timezone = raw.Timezone,
            TimezoneAbbreviation = raw.Timezone_abbreviation,
            Elevation = raw.Elevation.ToString(CultureInfo.InvariantCulture),
            
            Daily_Units = new DailyUnitsDto
            {
                Time = raw.Daily_units.Time,
                Temperature_2m_Max = raw.Daily_units.Temperature_2m_max,
                Temperature_2m_Min = raw.Daily_units.Temperature_2m_min,
                Apparent_Temperature_Max = raw.Daily_units.Apparent_temperature_max,
                Apparent_Temperature_Min = raw.Daily_units.Apparent_temperature_min,
                Precipitation_Sum = raw.Daily_units.Precipitation_sum,
                Precipitation_Hours = raw.Daily_units.Precipitation_hours,
                Sunshine_Duration = raw.Daily_units.Sunshine_duration,
                Uv_Index_Max = raw.Daily_units.Uv_index_max,
                Wind_Speed_10m_Max = raw.Daily_units.Wind_speed_10m_max,
                Wind_Gusts_10m_Max = raw.Daily_units.Wind_gusts_10m_max,
                Et0_Fao_Evapotranspiration = raw.Daily_units.Et0_fao_evapotranspiration,
                Sunrise = raw.Daily_units.Sunrise,
                Sunset = raw.Daily_units.Sunset
            },
            
            Daily = new List<DailyDataDto>()
        };

        // Transform daily data arrays thành list of objects
        for (int i = 0; i < raw.Daily.Time.Count; i++)
        {
            result.Daily.Add(new DailyDataDto
            {
                Time = raw.Daily.Time[i],
                Temperature_2m_Max = raw.Daily.Temperature_2m_max[i].ToString(CultureInfo.InvariantCulture),
                Temperature_2m_Min = raw.Daily.Temperature_2m_min[i].ToString(CultureInfo.InvariantCulture),
                Apparent_Temperature_Max = raw.Daily.Apparent_temperature_max[i].ToString(CultureInfo.InvariantCulture),
                Apparent_Temperature_Min = raw.Daily.Apparent_temperature_min[i].ToString(CultureInfo.InvariantCulture),
                Precipitation_Sum = raw.Daily.Precipitation_sum[i].ToString(CultureInfo.InvariantCulture),
                Precipitation_Hours = raw.Daily.Precipitation_hours[i].ToString(CultureInfo.InvariantCulture),
                Sunshine_Duration = raw.Daily.Sunshine_duration[i].ToString(CultureInfo.InvariantCulture),
                Uv_Index_Max = raw.Daily.Uv_index_max[i].ToString(CultureInfo.InvariantCulture),
                Wind_Speed_10m_Max = raw.Daily.Wind_speed_10m_max[i].ToString(CultureInfo.InvariantCulture),
                Wind_Gusts_10m_Max = raw.Daily.Wind_gusts_10m_max[i].ToString(CultureInfo.InvariantCulture),
                Et0_Fao_Evapotranspiration = raw.Daily.Et0_fao_evapotranspiration[i].ToString(CultureInfo.InvariantCulture),
                Sunrise = raw.Daily.Sunrise[i],
                Sunset = raw.Daily.Sunset[i]
            });
        }

        return result;
    }

    /// <summary>
    /// Transform raw current response từ API thành custom CurrentWeatherResponseDto
    /// </summary>
    public static CurrentWeatherResponseDto TransformCurrentResponse(WeatherCurrent raw)
    {
        var result = new CurrentWeatherResponseDto
        {
            Latitude = raw.Latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = raw.Longitude.ToString(CultureInfo.InvariantCulture),
            GenerationTimeMs = raw.Generationtime_ms.ToString(CultureInfo.InvariantCulture),
            UtcOffsetSeconds = raw.Utc_offset_seconds.ToString(),
            Timezone = raw.Timezone,
            TimezoneAbbreviation = raw.Timezone_abbreviation,
            Elevation = raw.Elevation.ToString(CultureInfo.InvariantCulture),
            
            Current_Units = new CurrentUnitsDto
            {
                Time = raw.Current_units.Time,
                Interval = raw.Current_units.Interval,
                Temperature_2m = raw.Current_units.Temperature_2m,
                Apparent_Temperature = raw.Current_units.Apparent_temperature,
                Relative_Humidity_2m = raw.Current_units.Relative_humidity_2m,
                Precipitation = raw.Current_units.Precipitation,
                Wind_Speed_10m = raw.Current_units.Wind_speed_10m,
                Wind_Gusts_10m = raw.Current_units.Wind_gusts_10m,
                Uv_Index = raw.Current_units.Uv_index,
                Soil_Moisture_0_to_1cm = raw.Current_units.Soil_moisture_0_to_1cm,
                Soil_Moisture_3_to_9cm = raw.Current_units.Soil_moisture_3_to_9cm,
                Soil_Temperature_0cm = raw.Current_units.Soil_temperature_0cm
            },
            
            Current = new CurrentDataDto
            {
                Time = raw.Current.Time,
                Interval = raw.Current.Interval.ToString(),
                Temperature_2m = raw.Current.Temperature_2m.ToString(CultureInfo.InvariantCulture),
                Apparent_Temperature = raw.Current.Apparent_temperature.ToString(CultureInfo.InvariantCulture),
                Relative_Humidity_2m = raw.Current.Relative_humidity_2m.ToString(),
                Precipitation = raw.Current.Precipitation.ToString(CultureInfo.InvariantCulture),
                Wind_Speed_10m = raw.Current.Wind_speed_10m.ToString(CultureInfo.InvariantCulture),
                Wind_Gusts_10m = raw.Current.Wind_gusts_10m.ToString(CultureInfo.InvariantCulture),
                Uv_Index = raw.Current.Uv_index.ToString(CultureInfo.InvariantCulture),
                Soil_Moisture_0_to_1cm = raw.Current.Soil_moisture_0_to_1cm.ToString(CultureInfo.InvariantCulture),
                Soil_Moisture_3_to_9cm = raw.Current.Soil_moisture_3_to_9cm.ToString(CultureInfo.InvariantCulture),
                Soil_Temperature_0cm = raw.Current.Soil_temperature_0cm.ToString(CultureInfo.InvariantCulture)
            }
        };

        return result;
    }

    /// <summary>
    /// Transform raw historical weather data to structured format (no calculations here)
    /// </summary>
    public static WeatherHistorical TransformHistoricalWeatherData(WeatherHistorical raw)
    {
        // Infrastructure layer only transforms data structure, no business calculations
        return raw;
    }
}