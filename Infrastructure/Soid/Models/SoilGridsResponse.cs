namespace Infrastructure.CO2.Models;

/// <summary>
/// Raw response models để deserialize JSON từ SoilGrids API
/// </summary>
public class SoilGridsResponse
{
    public string Type { get; set; } = string.Empty;
    public SoilGridsGeometry Geometry { get; set; } = new();
    public SoilGridsProperties Properties { get; set; } = new();
    public decimal Query_time_s { get; set; }
}

public class SoilGridsGeometry
{
    public string Type { get; set; } = string.Empty;
    public List<decimal> Coordinates { get; set; } = new();
}

public class SoilGridsProperties
{
    public List<SoilLayer> Layers { get; set; } = new();
}

public class SoilLayer
{
    public string Name { get; set; } = string.Empty;
    public SoilUnitMeasure Unit_measure { get; set; } = new();
    public List<SoilDepth> Depths { get; set; } = new();
}

public class SoilUnitMeasure
{
    public int D_factor { get; set; }
    public string Mapped_units { get; set; } = string.Empty;
    public string Target_units { get; set; } = string.Empty;
    public string Uncertainty_unit { get; set; } = string.Empty;
}

public class SoilDepth
{
    public SoilRange Range { get; set; } = new();
    public string Label { get; set; } = string.Empty;
    public SoilValues Values { get; set; } = new();
}

public class SoilRange
{
    public int Top_depth { get; set; }
    public int Bottom_depth { get; set; }
    public string Unit_depth { get; set; } = string.Empty;
}

public class SoilValues
{
    public decimal Mean { get; set; }
}