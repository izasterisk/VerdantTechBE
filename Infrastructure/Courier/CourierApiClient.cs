using System.Text;
using System.Text.Json;
using BLL.DTO.Courier;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Courier.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Courier;

public class CourierApiClient : ICourierApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly int _shopId;
    private readonly int _timeoutSeconds;

    public CourierApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = Environment.GetEnvironmentVariable("GHN_TESTING_URL") ?? "https://dev-online-gateway.ghn.vn/shiip/public-api/v2";
        _token = Environment.GetEnvironmentVariable("GHN_TESTING_TOKEN") ?? throw new InvalidOperationException("GHN_TESTING_TOKEN không được cấu hình trong .env file");
        _shopId = int.Parse(Environment.GetEnvironmentVariable("GHN_SHOP_ID") ?? throw new InvalidOperationException("GHN_SHOP_ID không được cấu hình trong .env file"));
        _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("TIME_OUT_SECONDS") ?? "10");
        
        // Configure HttpClient timeout and headers
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("Token", _token);
        _httpClient.DefaultRequestHeaders.Add("ShopId", _shopId.ToString());
    }

    public async Task<List<CourierServicesResponseDTO>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken cancellationToken = default)
    {
        return await CourierApiHelpers.ExecuteApiRequestAsync(async () =>
        {
            var url = $"{_baseUrl}/shipping-order/available-services";
            // Create request body
            var requestBody = new
            {
                shop_id = _shopId,
                from_district = fromDistrictId,
                to_district = toDistrictId
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            var services = CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateArrayData(dataElement, "dịch vụ vận chuyển");
                
                var serviceList = new List<CourierServices>();
                foreach (var item in dataElement.EnumerateArray())
                {
                    var service = new CourierServices
                    {
                        ServiceId = item.GetProperty("service_id").GetInt32(),
                        ShortName = item.GetProperty("short_name").GetString() ?? string.Empty,
                        ServiceTypeId = item.GetProperty("service_type_id").GetInt32()
                    };
                    serviceList.Add(service);
                }
                return serviceList;
            }, "dịch vụ vận chuyển");
            
            // Map to DTO
            return services.Select(service => new CourierServicesResponseDTO
            {
                ServiceId = service.ServiceId,
                ShortName = service.ShortName,
                ServiceTypeId = service.ServiceTypeId
            }).ToList();
        }, "lấy danh sách dịch vụ vận chuyển");
    }

    public async Task<int> GetDeliveryDateAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, CancellationToken cancellationToken = default)
    {
        return await CourierApiHelpers.ExecuteApiRequestAsync(async () =>
        {
            var url = $"{_baseUrl}/shipping-order/leadtime";
            // Create request body
            var requestBody = new
            {
                from_district_id = fromDistrictId,
                from_ward_code = fromWardCode,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                service_id = serviceId
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            return CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateObjectData(dataElement, "thời gian giao hàng");
                return dataElement.GetProperty("leadtime").GetInt32();
            }, "thời gian giao hàng");
        }, "lấy thời gian giao hàng");
    }

    public async Task<int> GetShippingFeeAsync(int fromDistrictId, string fromWardCode, int toDistrictId, 
        string toWardCode, int serviceId, int serviceTypeId, int height, int length, int weight, int width, CancellationToken cancellationToken = default)
    {
        return await CourierApiHelpers.ExecuteApiRequestAsync(async () =>
        {
            var url = $"{_baseUrl}/shipping-order/fee";
            // Create request body
            var requestBody = new
            {
                from_district_id = fromDistrictId,
                from_ward_code = fromWardCode,
                service_id = serviceId,
                service_type_id = serviceTypeId,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                height = height,
                length = length,
                weight = weight,
                width = width
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            return CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateObjectData(dataElement, "phí vận chuyển");
                return dataElement.GetProperty("total").GetInt32();
            }, "phí vận chuyển");
        }, "lấy phí vận chuyển");
    }

    public async Task<CourierOrderCreateResponseDTO> CreateOrderAsync(string toName, string toPhone, string toAddress, int toDistrictId, string toWardCode, int weight, int length, int width, int height, int paymentTypeId, string note, int serviceTypeId, int serviceId, int codAmount, List<OrderItemsCreateDTO> items, CancellationToken cancellationToken = default)
    {
        return await CourierApiHelpers.ExecuteApiRequestAsync(async () =>
        {
            var url = $"{_baseUrl}/shipping-order/create";
            // Create request body
            var requestBody = new
            {
                to_name = toName,
                to_phone = toPhone,
                to_address = toAddress,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                weight = weight,
                length = length,
                width = width,
                height = height,
                payment_type_id = paymentTypeId,
                required_note = "KHONGCHOXEMHANG",
                note = note,
                service_type_id = serviceTypeId,
                service_id = serviceId,
                cod_amount = codAmount,
                items = items.Select(item => new
                {
                    name = item.Name,
                    code = item.Code,
                    quantity = item.Quantity,
                    weight = item.Weight,
                    length = item.Length,
                    width = item.Width,
                    height = item.Height
                }).ToList()
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            var orderCreate = CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateObjectData(dataElement, "tạo đơn hàng");
                
                // Get fee object
                var feeElement = dataElement.GetProperty("fee");
                
                return new OrderCreate
                {
                    OrderCode = dataElement.GetProperty("order_code").GetString() ?? string.Empty,
                    SortCode = dataElement.GetProperty("sort_code").GetString() ?? string.Empty,
                    TransType = dataElement.GetProperty("trans_type").GetString() ?? string.Empty,
                    WardEncode = dataElement.GetProperty("ward_encode").GetString() ?? string.Empty,
                    DistrictEncode = dataElement.GetProperty("district_encode").GetString() ?? string.Empty,
                    MainService = feeElement.GetProperty("main_service").GetInt32(),
                    Insurance = feeElement.GetProperty("insurance").GetInt32(),
                    CodFee = feeElement.GetProperty("cod_fee").GetInt32(),
                    StationDo = feeElement.GetProperty("station_do").GetInt32(),
                    StationPu = feeElement.GetProperty("station_pu").GetInt32(),
                    Return = feeElement.GetProperty("return").GetInt32(),
                    R2s = feeElement.GetProperty("r2s").GetInt32(),
                    ReturnAgain = feeElement.GetProperty("return_again").GetInt32(),
                    Coupon = feeElement.GetProperty("coupon").GetInt32(),
                    DocumentReturn = feeElement.GetProperty("document_return").GetInt32(),
                    DoubleCheck = feeElement.GetProperty("double_check").GetInt32(),
                    DoubleCheckDeliver = feeElement.GetProperty("double_check_deliver").GetInt32(),
                    PickRemoteAreasFee = feeElement.GetProperty("pick_remote_areas_fee").GetInt32(),
                    DeliverRemoteAreasFee = feeElement.GetProperty("deliver_remote_areas_fee").GetInt32(),
                    PickRemoteAreasFeeReturn = feeElement.GetProperty("pick_remote_areas_fee_return").GetInt32(),
                    DeliverRemoteAreasFeeReturn = feeElement.GetProperty("deliver_remote_areas_fee_return").GetInt32(),
                    CodFailedFee = feeElement.GetProperty("cod_failed_fee").GetInt32(),
                    TotalFee = dataElement.GetProperty("total_fee").GetInt32(),
                    ExpectedDeliveryTime = DateTime.Parse(dataElement.GetProperty("expected_delivery_time").GetString() ?? string.Empty),
                    OperationPartner = dataElement.GetProperty("operation_partner").GetString() ?? string.Empty
                };
            }, "tạo đơn hàng");
            
            // Map to DTO
            return new CourierOrderCreateResponseDTO
            {
                OrderCode = orderCreate.OrderCode,
                TransType = orderCreate.TransType,
                TotalFee = orderCreate.TotalFee,
                ExpectedDeliveryTime = orderCreate.ExpectedDeliveryTime
            };
        }, "tạo đơn hàng");
    }
}