namespace BLL.DTO.Weather;

public class DailyWeatherResponseDto
{
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string GenerationTimeMs { get; set; } = string.Empty;
    public string UtcOffsetSeconds { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string TimezoneAbbreviation { get; set; } = string.Empty;
    public string Elevation { get; set; } = string.Empty;

    public DailyUnitsDto Daily_Units { get; set; } = new();
    public List<DailyDataDto> Daily { get; set; } = new();
}

public class DailyUnitsDto
{
    public string Time { get; set; }
    public string Temperature_2m_Max { get; set; }
    public string Temperature_2m_Min { get; set; }
    public string Apparent_Temperature_Max { get; set; }
    public string Apparent_Temperature_Min { get; set; }
    public string Precipitation_Sum { get; set; }
    public string Precipitation_Hours { get; set; }
    public string Sunshine_Duration { get; set; }
    public string Uv_Index_Max { get; set; }
    public string Wind_Speed_10m_Max { get; set; }
    public string Wind_Gusts_10m_Max { get; set; }
    public string Et0_Fao_Evapotranspiration { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
}

public class DailyDataDto
{
    public string Time { get; set; }
    public string Temperature_2m_Max { get; set; }
    public string Temperature_2m_Min { get; set; }
    public string Apparent_Temperature_Max { get; set; }
    public string Apparent_Temperature_Min { get; set; }
    public string Precipitation_Sum { get; set; }
    public string Precipitation_Hours { get; set; }
    public string Sunshine_Duration { get; set; }
    public string Uv_Index_Max { get; set; }
    public string Wind_Speed_10m_Max { get; set; }
    public string Wind_Gusts_10m_Max { get; set; }
    public string Et0_Fao_Evapotranspiration { get; set; }
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
}
