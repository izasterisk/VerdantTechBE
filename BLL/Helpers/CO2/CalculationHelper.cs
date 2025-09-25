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
}