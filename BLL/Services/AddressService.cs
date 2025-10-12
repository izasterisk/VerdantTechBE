using BLL.DTO.Address;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;

namespace BLL.Services;

public class AddressService : IAddressService
{
    private readonly IAddressApiClient _addressApiClient;

    public AddressService(IAddressApiClient addressApiClient)
    {
        _addressApiClient = addressApiClient;
    }

    public async Task<List<CourierProvinceResponseDTO>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        return await _addressApiClient.GetProvincesAsync(cancellationToken);
    }

    public async Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(int? provinceId = null, CancellationToken cancellationToken = default)
    {
        var allDistricts = await _addressApiClient.GetDistrictsAsync(cancellationToken);
        if (provinceId.HasValue)
        {
            return allDistricts.Where(d => d.ProvinceId == provinceId.Value).ToList();
        }
        return allDistricts;
    }

    public async Task<List<CourierCommuneResponseDTO>> GetCommunesAsync(int districtId, CancellationToken cancellationToken = default)
    {
        return await _addressApiClient.GetCommunesAsync(districtId, cancellationToken);
    }
}