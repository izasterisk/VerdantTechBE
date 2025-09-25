using BLL.Interfaces.Infrastructure;

namespace Infrastructure.CO2;

public static class SoilGridsHelper
{
    /// <summary>
    /// Build URL for SoilGrids API query
    /// </summary>
    public static string BuildSoilGridsUrl(string baseUrl, decimal longitude, decimal latitude)
    {
        var properties = "property=phh2o&property=sand&property=silt&property=clay";
        var depths = "depth=0-5cm&depth=5-15cm&depth=15-30cm";
        var value = "value=mean";
        
        return $"{baseUrl}query?lon={longitude:F4}&lat={latitude:F4}&{properties}&{depths}&{value}";
    }
    
    /// <summary>
    /// Convert raw value to percentage or actual value based on d_factor
    /// For sand, silt, clay: divide by d_factor (10) to convert g/kg to %
    /// For pH: divide by d_factor (10) to get actual pH value
    /// </summary>
    public static decimal ConvertValue(decimal rawValue, int dFactor)
    {
        return rawValue / dFactor;
    }
    
    /// <summary>
    /// Validate that all soil parameters have valid values
    /// </summary>
    public static void ValidateSoilData(
        decimal sand0_5, decimal sand5_15, decimal sand15_30,
        decimal silt0_5, decimal silt5_15, decimal silt15_30,
        decimal clay0_5, decimal clay5_15, decimal clay15_30,
        decimal ph0_5, decimal ph5_15, decimal ph15_30)
    {
        // Check for any invalid percentage values (sand, silt, clay)
        if (HasInvalidPercentageValue(sand0_5) || HasInvalidPercentageValue(sand5_15) || HasInvalidPercentageValue(sand15_30) ||
            HasInvalidPercentageValue(silt0_5) || HasInvalidPercentageValue(silt5_15) || HasInvalidPercentageValue(silt15_30) ||
            HasInvalidPercentageValue(clay0_5) || HasInvalidPercentageValue(clay5_15) || HasInvalidPercentageValue(clay15_30))
        {
            throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
        }
        
        // Check for any invalid pH values
        if (HasInvalidPhValue(ph0_5) || HasInvalidPhValue(ph5_15) || HasInvalidPhValue(ph15_30))
        {
            throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
        }
    }
    
    /// <summary>
    /// Check if a percentage value (sand, silt, clay) is invalid
    /// </summary>
    private static bool HasInvalidPercentageValue(decimal value)
    {
        return value <= 0 || value > 100;
    }
    
    /// <summary>
    /// Check if a pH value is invalid (should be between 0 and 14)
    /// </summary>
    private static bool HasInvalidPhValue(decimal value)
    {
        return value <= 0 || value > 14;
    }
}