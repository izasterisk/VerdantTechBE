using Infrastructure.CO2.Models;
using BLL.Interfaces.Infrastructure;

namespace Infrastructure.CO2;

public static class SoilGridsResponseTransformer
{
    /// <summary>
    /// Transform raw SoilGrids response to structured raw data (no calculations)
    /// </summary>
    public static SoilDataResult TransformSoilGridsResponse(SoilGridsResponse rawResponse)
    {
        var result = new SoilDataResult();
        
        foreach (var layer in rawResponse.Properties.Layers)
        {
            if (layer.Depths.Count != 3)
                throw new InvalidOperationException($"Expected 3 depth layers for {layer.Name}, got {layer.Depths.Count}");
            
            // Extract raw values for each depth layer (convert using d_factor only)
            var layer0_5 = SoilGridsHelper.ConvertValue(layer.Depths[0].Values.Mean, layer.Unit_measure.D_factor);
            var layer5_15 = SoilGridsHelper.ConvertValue(layer.Depths[1].Values.Mean, layer.Unit_measure.D_factor);
            var layer15_30 = SoilGridsHelper.ConvertValue(layer.Depths[2].Values.Mean, layer.Unit_measure.D_factor);
            
            // Store raw layer data
            switch (layer.Name.ToLower())
            {
                case "sand":
                    result.SandLayers = new List<decimal> { layer0_5, layer5_15, layer15_30 };
                    break;
                case "silt":
                    result.SiltLayers = new List<decimal> { layer0_5, layer5_15, layer15_30 };
                    break;
                case "clay":
                    result.ClayLayers = new List<decimal> { layer0_5, layer5_15, layer15_30 };
                    break;
                case "phh2o":
                    result.PhLayers = new List<decimal> { layer0_5, layer5_15, layer15_30 };
                    break;
                default:
                    // Ignore unknown properties
                    break;
            }
        }
        
        return result;
    }
}