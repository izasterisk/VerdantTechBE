using BLL.DTO.Courier;

namespace BLL.Interfaces.Infrastructure;

public interface ICourierApiClient
{
    Task<List<CourierServicesResponseDTO>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken cancellationToken = default);
    Task<int> GetDeliveryDateAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, CancellationToken cancellationToken = default);
    Task<int> GetShippingFeeAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, int serviceTypeId, int height, int length, int weight, int width, CancellationToken cancellationToken = default);
}