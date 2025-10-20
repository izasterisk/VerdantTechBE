using BLL.DTO.Address;

namespace BLL.Interfaces.Infrastructure;

public interface IGHNAddressApiClient
{
    Task<(string ProvinceCode, string DistrictCode, string CommuneCode)> GetGHNAddressCodesAsync(string provinceName,
        string districtName, string communeName, CancellationToken cancellationToken = default);
}