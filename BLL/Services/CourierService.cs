using BLL.DTO.Courier;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;

namespace BLL.Services;

public class CourierService : ICourierService
{
    private readonly ICourierApiClient _courierApiClient;

    public CourierService(ICourierApiClient courierApiClient)
    {
        _courierApiClient = courierApiClient;
    }

    public async Task<List<CourierProvinceResponseDTO>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _courierApiClient.GetCitiesAsync(cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Không thể lấy danh sách tỉnh/thành phố. Vui lòng thử lại sau.");
        }
    }

    public async Task<List<CourierDistrictResponseDTO>> GetDistrictsByCityIdAsync(string cityId, CancellationToken cancellationToken = default)
    {
        // Validate cityId format
        if (string.IsNullOrEmpty(cityId) || cityId.Length != 6 || !cityId.All(char.IsDigit))
        {
            throw new ArgumentException("Mã thành phố không hợp lệ. Mã thành phố phải có 6 chữ số.");
        }

        try
        {
            return await _courierApiClient.GetDistrictsAsync(cityId, cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException($"Không thể lấy danh sách quận/huyện cho thành phố {cityId}. Vui lòng kiểm tra mã thành phố và thử lại.");
        }
    }

    public async Task<List<CourierWardResponseDTO>> GetWardsByDistrictIdAsync(string districtId, CancellationToken cancellationToken = default)
    {
        // Validate districtId format
        if (string.IsNullOrEmpty(districtId) || districtId.Length != 6 || !districtId.All(char.IsDigit))
        {
            throw new ArgumentException("Mã quận/huyện không hợp lệ. Mã quận/huyện phải có 6 chữ số.");
        }

        try
        {
            return await _courierApiClient.GetWardsAsync(districtId, cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException($"Không thể lấy danh sách phường/xã cho quận/huyện {districtId}. Vui lòng kiểm tra mã quận/huyện và thử lại.");
        }
    }
}