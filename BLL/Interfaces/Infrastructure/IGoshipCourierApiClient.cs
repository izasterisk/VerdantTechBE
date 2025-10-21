using BLL.DTO.Courier;

namespace BLL.Interfaces.Infrastructure;

public interface IGoshipCourierApiClient
{
    Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrict, string fromCity, string toDistrict,
        string toCity, decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight,
        CancellationToken cancellationToken = default);
}