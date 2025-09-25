namespace BLL.Helpers.CO2;

public static class CalculationHelper
{
    /// <summary>
    /// Business logic: Calculate weighted average for soil properties across depth layers
    /// Formula: (layer0-5 × 5 + layer5-15 × 10 + layer15-30 × 15) ÷ 30
    /// </summary>
    public static decimal CalculateWeightedAverage(decimal layer0_5, decimal layer5_15, decimal layer15_30)
    {
        return (layer0_5 * 5 + layer5_15 * 10 + layer15_30 * 15) / 30;
    }

    /// <summary>
    /// Business logic: Calculate average from weather data list, filtering out null values
    /// </summary>
    /// <param name="values">List of nullable decimal values from weather API</param>
    /// <returns>Average of non-null values, or 0 if all values are null</returns>
    public static decimal CalculateAverage(IEnumerable<decimal?> values)
    {
        var validValues = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        
        if (!validValues.Any())
        {
            return 0;
        }
        
        return validValues.Average();
    }

    /// <summary>
    /// Business logic: Calculate historical weather averages from simple arrays
    /// </summary>
    public static (decimal precipitationAvg, decimal et0Avg) CalculateHistoricalWeatherAverages(decimal?[] precipitationData, decimal?[] et0Data)
    {
        var precipitationAvg = CalculateAverage(precipitationData);
        var et0Avg = CalculateAverage(et0Data);

        // Business validation: check if location is supported
        if (precipitationAvg == 0 && et0Avg == 0)
        {
            throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
        }

        return (precipitationAvg, et0Avg);
    }
}