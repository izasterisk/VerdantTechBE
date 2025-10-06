using BLL.DTO.Courier;

namespace BLL.Interfaces.Infrastructure;

public interface ICourierApiClient
{
    Task<List<CourierProvinceResponseDTO>> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(string cityId, CancellationToken cancellationToken = default);
    Task<List<CourierWardResponseDTO>> GetWardsAsync(string districtId, CancellationToken cancellationToken = default);
    Task<List<RateResponseDTO>> GetRatesAsync(int fromDistrict, int fromCity, int toDistrict, int toCity, 
        decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight, 
        CancellationToken cancellationToken = default);
}