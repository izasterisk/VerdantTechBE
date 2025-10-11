using BLL.DTO.Courier;

namespace BLL.Interfaces.Infrastructure;

public interface ICourierApiClient
{
    Task<List<RateResponseDTO>> GetRatesAsync(int fromDistrict, int fromCity, int toDistrict, int toCity, 
        decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight, 
        CancellationToken cancellationToken = default);
}