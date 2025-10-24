using BLL.DTO.Courier;
using BLL.DTO.User;

namespace BLL.Interfaces.Infrastructure;

public interface IGoshipCourierApiClient
{
    Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrictCode, string fromCityCode, string toDistrictCode,
         string toCityCode, int codAmount, int width, int height, int length, int weight,
         CancellationToken cancellationToken = default);

    Task<string> CreateShipmentAsync(UserResponseDTO from, UserResponseDTO to,
        int codAmount, int length, int width, int height, int weight, int amount,
        int payer, int priceTableId, string metadata, CancellationToken cancellationToken = default);
}