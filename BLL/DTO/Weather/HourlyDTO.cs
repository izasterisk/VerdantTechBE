namespace BLL.DTO.Weather;

public class HourlyWeatherResponseDto
{
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string GenerationTimeMs { get; set; }
    public string UtcOffsetSeconds { get; set; }
    public string Timezone { get; set; }
    public string TimezoneAbbreviation { get; set; }
    public string Elevation { get; set; }

    public HourlyUnitsDto Hourly_Units { get; set; }
    public List<HourlyDataDto> Hourly { get; set; }
}

public class HourlyUnitsDto
{
    public string Time { get; set; }
    public string Temperature_2m { get; set; }
    public string Apparent_Temperature { get; set; }
    public string Relative_Humidity_2m { get; set; }
    public string Precipitation { get; set; }
    public string Wind_Speed_10m { get; set; }
    public string Wind_Gusts_10m { get; set; }
    public string Uv_Index { get; set; }
    public string Soil_Moisture_0_to_1cm { get; set; }
    public string Soil_Moisture_3_to_9cm { get; set; }
    public string Soil_Temperature_0cm { get; set; }
}

public class HourlyDataDto
{
    public string Time { get; set; }
    public string Temperature_2m { get; set; }
    public string Apparent_Temperature { get; set; }
    public string Relative_Humidity_2m { get; set; }
    public string Precipitation { get; set; }
    public string Wind_Speed_10m { get; set; }
    public string Wind_Gusts_10m { get; set; }
    public string Uv_Index { get; set; }
    public string Soil_Moisture_0_to_1cm { get; set; }
    public string Soil_Moisture_3_to_9cm { get; set; }
    public string Soil_Temperature_0cm { get; set; }
}
