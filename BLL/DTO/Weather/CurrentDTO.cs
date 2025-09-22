namespace BLL.DTO.Weather;

public class CurrentWeatherResponseDto
{
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string GenerationTimeMs { get; set; } = string.Empty;
    public string UtcOffsetSeconds { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string TimezoneAbbreviation { get; set; } = string.Empty;
    public string Elevation { get; set; } = string.Empty;

    public CurrentUnitsDto Current_Units { get; set; } = new();
    public CurrentDataDto Current { get; set; } = new();
}

public class CurrentUnitsDto
{
    public string Time { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public string Temperature_2m { get; set; } = string.Empty;
    public string Apparent_Temperature { get; set; } = string.Empty;
    public string Relative_Humidity_2m { get; set; } = string.Empty;
    public string Precipitation { get; set; } = string.Empty;
    public string Wind_Speed_10m { get; set; } = string.Empty;
    public string Wind_Gusts_10m { get; set; } = string.Empty;
    public string Uv_Index { get; set; } = string.Empty;
    public string Soil_Moisture_0_to_1cm { get; set; } = string.Empty;
    public string Soil_Moisture_3_to_9cm { get; set; } = string.Empty;
    public string Soil_Temperature_0cm { get; set; } = string.Empty;
}

public class CurrentDataDto
{
    public string Time { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public string Temperature_2m { get; set; } = string.Empty;
    public string Apparent_Temperature { get; set; } = string.Empty;
    public string Relative_Humidity_2m { get; set; } = string.Empty;
    public string Precipitation { get; set; } = string.Empty;
    public string Wind_Speed_10m { get; set; } = string.Empty;
    public string Wind_Gusts_10m { get; set; } = string.Empty;
    public string Uv_Index { get; set; } = string.Empty;
    public string Soil_Moisture_0_to_1cm { get; set; } = string.Empty;
    public string Soil_Moisture_3_to_9cm { get; set; } = string.Empty;
    public string Soil_Temperature_0cm { get; set; } = string.Empty;
}