using BLL.DTO.Address;

namespace BLL.Interfaces.Infrastructure;

public interface IAddressApiClient
{
    Task<List<CourierProvinceResponseDTO>> GetProvincesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(CancellationToken cancellationToken = default);
    Task<List<CourierCommuneResponseDTO>> GetCommunesAsync(int districtId, CancellationToken cancellationToken = default);
}