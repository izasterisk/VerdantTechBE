namespace BLL.Interfaces.Infrastructure;

public interface ISoilGridsApiClient
{
    Task<SoilDataResult> GetSoilDataAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
}

/// <summary>
/// Raw soil data from SoilGrids API with layer values
/// </summary>
public class SoilDataResult
{
    public List<decimal> SandLayers { get; set; } = new(); // [0-5cm, 5-15cm, 15-30cm]
    public List<decimal> SiltLayers { get; set; } = new(); // [0-5cm, 5-15cm, 15-30cm]
    public List<decimal> ClayLayers { get; set; } = new(); // [0-5cm, 5-15cm, 15-30cm]
    public List<decimal> PhLayers { get; set; } = new();   // [0-5cm, 5-15cm, 15-30cm]
}