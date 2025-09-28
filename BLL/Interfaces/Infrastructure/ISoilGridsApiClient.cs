using BLL.DTO.Soil;

namespace BLL.Interfaces.Infrastructure;

public interface ISoilGridsApiClient
{
    Task<SoilDataResult> GetSoilDataAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
}