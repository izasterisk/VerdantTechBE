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
        try
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
            
            // Parse GHN response
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            // Check response code
            if (!root.TryGetProperty("code", out var codeElement) || codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() 
                    : "Unknown error";
                throw new InvalidOperationException($"GHN API trả về lỗi: {message}");
            }
            
            // Get data array
            if (!root.TryGetProperty("data", out var dataElement))
            {
                throw new InvalidOperationException("Response từ GHN không chứa trường 'data'");
            }
            
            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Trường 'data' không phải là một mảng");
            }
            
            // Deserialize to model
            var services = new List<CourierServices>();
            foreach (var item in dataElement.EnumerateArray())
            {
                var service = new CourierServices
                {
                    ServiceId = item.GetProperty("service_id").GetInt32(),
                    ShortName = item.GetProperty("short_name").GetString() ?? string.Empty,
                    ServiceTypeId = item.GetProperty("service_type_id").GetInt32()
                };
                services.Add(service);
            }
            
            // Map to DTO
            return services.Select(service => new CourierServicesResponseDTO
            {
                ServiceId = service.ServiceId,
                ShortName = service.ShortName,
                ServiceTypeId = service.ServiceTypeId
            }).ToList();
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GHN hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GHN: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách dịch vụ vận chuyển: {ex.Message}");
        }
    }

    public async Task<int> GetDeliveryDateAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, CancellationToken cancellationToken = default)
    {
        try
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
            var leadTime = CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateObjectData(dataElement, "thời gian giao hàng");
                return dataElement.GetProperty("leadtime").GetInt32();
            }, "thời gian giao hàng");
            
            return leadTime;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GHN hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GHN: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy thời gian giao hàng: {ex.Message}");
        }
    }

    public async Task<int> GetShippingFeeAsync(int fromDistrictId, string fromWardCode, int toDistrictId, string toWardCode, int serviceId, int serviceTypeId, int height, int length, int weight, int width, CancellationToken cancellationToken = default)
    {
        try
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
            var total = CourierApiHelpers.ParseGhnResponse(responseContent, dataElement =>
            {
                CourierApiHelpers.ValidateObjectData(dataElement, "phí vận chuyển");
                
                // Get total fee
                return dataElement.GetProperty("total").GetInt32();
            }, "phí vận chuyển");
            
            return total;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GHN hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GHN: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy phí vận chuyển: {ex.Message}");
        }
    }
}