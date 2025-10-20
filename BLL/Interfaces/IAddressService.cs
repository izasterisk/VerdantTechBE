using BLL.DTO.Address;

namespace BLL.Interfaces;

public interface IAddressService
{
    Task<List<CourierProvinceResponseDTO>> GetProvincesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(string provinceId, CancellationToken cancellationToken = default);
    Task<List<CourierCommuneResponseDTO>> GetCommunesAsync(string districtId, CancellationToken cancellationToken = default);
}