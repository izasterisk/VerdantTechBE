using BLL.DTO.Address;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;

namespace BLL.Services;

public class AddressService : IAddressService
{
    private readonly IGoshipAddressApiClient _addressApiClient;

    public AddressService(IGoshipAddressApiClient addressApiClient)
    {
        _addressApiClient = addressApiClient;
    }

    public async Task<List<CourierProvinceResponseDTO>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        return await _addressApiClient.GoshipGetProvincesAsync(cancellationToken);
    }

    public async Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(string provinceId, CancellationToken cancellationToken = default)
    {
        return await _addressApiClient.GoshipGetDistrictsAsync(provinceId, cancellationToken);
    }

    public async Task<List<CourierCommuneResponseDTO>> GetCommunesAsync(string districtId, CancellationToken cancellationToken = default)
    {
        return await _addressApiClient.GoshipGetCommunesAsync(districtId, cancellationToken);
    }
}