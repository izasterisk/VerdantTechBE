using System.Collections.Generic;

namespace BLL.DTO.Weather
{
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
        public string Time { get; set; } = string.Empty;
        public string Temperature_2m_Max { get; set; } = string.Empty;
        public string Temperature_2m_Min { get; set; } = string.Empty;
        public string Apparent_Temperature_Max { get; set; } = string.Empty;
        public string Apparent_Temperature_Min { get; set; } = string.Empty;
        public string Precipitation_Sum { get; set; } = string.Empty;
        public string Precipitation_Hours { get; set; } = string.Empty;
        public string Sunshine_Duration { get; set; } = string.Empty;
        public string Uv_Index_Max { get; set; } = string.Empty;
        public string Wind_Speed_10m_Max { get; set; } = string.Empty;
        public string Wind_Gusts_10m_Max { get; set; } = string.Empty;
        public string Et0_Fao_Evapotranspiration { get; set; } = string.Empty;
        public string Sunrise { get; set; } = string.Empty;
        public string Sunset { get; set; } = string.Empty;
    }

    public class DailyDataDto
    {
        public string Time { get; set; } = string.Empty;
        public string Temperature_2m_Max { get; set; } = string.Empty;
        public string Temperature_2m_Min { get; set; } = string.Empty;
        public string Apparent_Temperature_Max { get; set; } = string.Empty;
        public string Apparent_Temperature_Min { get; set; } = string.Empty;
        public string Precipitation_Sum { get; set; } = string.Empty;
        public string Precipitation_Hours { get; set; } = string.Empty;
        public string Sunshine_Duration { get; set; } = string.Empty;
        public string Uv_Index_Max { get; set; } = string.Empty;
        public string Wind_Speed_10m_Max { get; set; } = string.Empty;
        public string Wind_Gusts_10m_Max { get; set; } = string.Empty;
        public string Et0_Fao_Evapotranspiration { get; set; } = string.Empty;
        public string Sunrise { get; set; } = string.Empty;
        public string Sunset { get; set; } = string.Empty;
    }
}
