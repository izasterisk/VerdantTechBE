using BLL.DTO.Courier;
using BLL.DTO.User;

namespace BLL.Interfaces.Infrastructure;

public interface IGoshipCourierApiClient
{
    Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrict, string fromCity, string toDistrict,
        string toCity, decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight,
        CancellationToken cancellationToken = default);

    Task<string> CreateShipmentAsync(string rate, int payer, UserResponseDTO from, UserResponseDTO to,
        int cod, int amount, string weight, string width, string height, string length, string metadata,
        CancellationToken cancellationToken = default);
}