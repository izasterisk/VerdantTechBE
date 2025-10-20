using BLL.DTO.Address;

namespace BLL.Interfaces.Infrastructure;

public interface IAddressApiClient
{
    Task<List<CourierProvinceResponseDTO>> GHNGetProvincesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GHNGetDistrictsAsync(CancellationToken cancellationToken = default);
    Task<List<CourierCommuneResponseDTO>> GHNGetCommunesAsync(int districtId, CancellationToken cancellationToken = default);
}