using BLL.DTO.Courier;

namespace BLL.Interfaces.Infrastructure;

public interface ICourierApiClient
{
    Task<List<CourierServicesResponseDTO>> GHNGetAvailableServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken cancellationToken = default);
    Task<int> GHNGetDeliveryDateAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, CancellationToken cancellationToken = default);
    Task<int> GHNGetShippingFeeAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, int serviceTypeId, int height, int length, int weight, int width, CancellationToken cancellationToken = default);
    Task<CourierOrderCreateResponseDTO> GHNCreateOrderAsync(string toName, string toPhone, string toAddress, int toDistrictId, string toWardCode, int weight, int length, int width, int height, int paymentTypeId, string note, int serviceTypeId, int serviceId, int codAmount, List<OrderItemsCreateDTO> items, CancellationToken cancellationToken = default);
}