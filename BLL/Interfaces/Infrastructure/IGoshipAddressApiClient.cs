using BLL.DTO.Address;

namespace BLL.Interfaces.Infrastructure;

public interface IGoshipAddressApiClient
{
    Task<List<CourierProvinceResponseDTO>> GoshipGetProvincesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GoshipGetDistrictsAsync(string cityId, CancellationToken cancellationToken = default);
    Task<List<CourierCommuneResponseDTO>> GoshipGetCommunesAsync(string districtId, CancellationToken cancellationToken = default);
}