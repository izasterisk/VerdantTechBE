using BLL.DTO.Courier;
using BLL.DTO.User;

namespace BLL.Interfaces.Infrastructure;

public interface IGoshipCourierApiClient
{
    Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrictCode, string fromCityCode, string toDistrictCode,
         string toCityCode, int codAmount, int width, int height, int length, int weight,
         CancellationToken cancellationToken = default);

    Task<string> CreateShipmentAsync(string rate, int payer, UserResponseDTO from, UserResponseDTO to,
        int cod, int amount, string weight, string width, string height, string length, string metadata,
        CancellationToken cancellationToken = default);
}