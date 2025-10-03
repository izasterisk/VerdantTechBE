using BLL.DTO.Courier;

namespace BLL.Interfaces;

public interface ICourierService
{
    Task<List<CourierProvinceResponseDTO>> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task<List<CourierDistrictResponseDTO>> GetDistrictsByCityIdAsync(string cityId, CancellationToken cancellationToken = default);
    Task<List<CourierWardResponseDTO>> GetWardsByDistrictIdAsync(string districtId, CancellationToken cancellationToken = default);
}