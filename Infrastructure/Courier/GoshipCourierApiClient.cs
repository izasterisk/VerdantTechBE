using System.Text.Json;
using BLL.DTO.Courier;
using BLL.DTO.User;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Address;
using Infrastructure.Courier.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Courier;

public class GoshipCourierApiClient : IGoshipCourierApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _bearerToken;
    private readonly int _timeoutSeconds;
    
    public GoshipCourierApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = Environment.GetEnvironmentVariable("GOSHIP_SANDBOX_MANAGE_API_ENDPOINT") ?? "https://sandbox.goship.io/api/v2";
        _bearerToken = Environment.GetEnvironmentVariable("GOSHIP_TOKEN") ?? throw new InvalidOperationException("GOSHIP_TOKEN không được cấu hình trong .env file");
        _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("TIME_OUT_SECONDS") ?? "10");
        
        // Configure HttpClient timeout and Bearer token
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
    }
    
    public async Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrict, string fromCity, string toDistrict, 
        string toCity, int cod, int amount, int width, int height, int length, int weight, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/rates";
            
            var requestBody = new
            {
                shipment = new
                {
                    address_from = new
                    {
                        district = fromDistrict,
                        city = fromCity
                    },
                    address_to = new
                    {
                        district = toDistrict,
                        city = toCity
                    },
                    parcel = new
                    {
                        cod = cod,
                        amount = amount,
                        width = width,
                        height = height,
                        length = length,
                        weight = weight
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var dataElement = AddressApiHelpers.ParseGoshipResponse(responseContent, "rates");
            
            var rates = JsonSerializer.Deserialize<List<RateResponse>>(
                dataElement.GetRawText(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (rates == null) return new List<RateResponseDTO>();
            return rates.Select(rate => new RateResponseDTO
            {
                Id = rate.Id,
                CarrierName = rate.CarrierName,
                CarrierLogo = rate.CarrierLogo,
                CarrierShortName = rate.CarrierShortName,
                Service = rate.Service,
                Expected = rate.Expected,
                IsApplyOnly = rate.IsApplyOnly,
                PromotionId = rate.PromotionId,
                Discount = rate.Discount,
                WeightFee = rate.WeightFee,
                LocationFirstFee = rate.LocationFirstFee,
                LocationStepFee = rate.LocationStepFee,
                RemoteAreaFee = rate.RemoteAreaFee,
                OilFee = rate.OilFee,
                LocationFee = rate.LocationFee,
                CodFee = rate.CodFee,
                ServiceFee = rate.ServiceFee,
                TotalFee = rate.TotalFee,
                TotalAmount = rate.TotalAmount,
                TotalAmountCarrier = rate.TotalAmountCarrier,
                TotalAmountShop = rate.TotalAmountShop,
                PriceTableId = rate.PriceTableId,
                InsurranceFee = rate.InsurranceFee,
                ReturnFee = rate.ReturnFee
            }).ToList();
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "rates");
            throw; 
        }
    }
    
    public async Task<string> CreateShipmentAsync(string rate, int payer, UserResponseDTO from, UserResponseDTO to,
        int cod, int amount, string weight, string width, string height, string length, string metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/shipments";
            
            // Lấy địa chỉ đầu tiên từ UserResponseDTO
            var addressFrom = from.Address.FirstOrDefault() 
                ?? throw new InvalidOperationException("Người gửi không có địa chỉ.");
            var addressTo = to.Address.FirstOrDefault() 
                ?? throw new InvalidOperationException("Người nhận không có địa chỉ.");
            
            var requestBody = new
            {
                shipment = new
                {
                    rate = rate,
                    payer = payer,
                    address_from = new
                    {
                        name = from.FullName,
                        phone = from.PhoneNumber ?? throw new InvalidOperationException("Người gửi không có số điện thoại."),
                        street = addressFrom.LocationAddress ?? throw new InvalidOperationException("Địa chỉ người gửi không có thông tin đường."),
                        ward = addressFrom.CommuneCode ?? throw new InvalidOperationException("Địa chỉ người gửi không có mã xã/phường."),
                        district = addressFrom.DistrictCode ?? throw new InvalidOperationException("Địa chỉ người gửi không có mã quận/huyện."),
                        city = addressFrom.ProvinceCode ?? throw new InvalidOperationException("Địa chỉ người gửi không có mã tỉnh/thành phố.")
                    },
                    address_to = new
                    {
                        name = to.FullName,
                        phone = to.PhoneNumber ?? throw new InvalidOperationException("Người nhận không có số điện thoại."),
                        street = addressTo.LocationAddress ?? throw new InvalidOperationException("Địa chỉ người nhận không có thông tin đường."),
                        ward = addressTo.CommuneCode ?? throw new InvalidOperationException("Địa chỉ người nhận không có mã xã/phường."),
                        district = addressTo.DistrictCode ?? throw new InvalidOperationException("Địa chỉ người nhận không có mã quận/huyện."),
                        city = addressTo.ProvinceCode ?? throw new InvalidOperationException("Địa chỉ người nhận không có mã tỉnh/thành phố.")
                    },
                    parcel = new
                    {
                        cod = cod,
                        amount = amount,
                        weight = weight,
                        width = width,
                        height = height,
                        length = length,
                        metadata = metadata
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Validate response không phải HTML
            if (responseContent.TrimStart().StartsWith("<"))
            {
                throw new HttpRequestException("API endpoint không khả dụng hoặc định dạng request không đúng. Server trả về HTML thay vì JSON.");
            }
            
            // Parse JSON response
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(responseContent);
            }
            catch (JsonException ex)
            {
                throw new HttpRequestException($"Không thể parse JSON response từ GoShip API khi tạo shipment: {ex.Message}");
            }
            
            var root = document.RootElement;
            
            // Kiểm tra code response
            if (!root.TryGetProperty("code", out var codeElement) || codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() 
                    : "Unknown error";
                    
                // Lấy chi tiết lỗi validation nếu có
                if (root.TryGetProperty("data", out var dataElement) && 
                    dataElement.TryGetProperty("errors", out var errorsElement))
                {
                    var errorDetails = errorsElement.GetRawText();
                    throw new HttpRequestException($"GoShip API error khi tạo shipment: {message}. Chi tiết: {errorDetails}");
                }
                
                throw new HttpRequestException($"GoShip API error khi tạo shipment: {message}");
            }
            
            // Lấy id từ response
            if (!root.TryGetProperty("id", out var idElement))
            {
                throw new HttpRequestException("Response không có trường 'id' khi tạo shipment.");
            }
            
            return idElement.GetString() ?? throw new HttpRequestException("Shipment ID là null.");
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HttpRequestException
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Lỗi không xác định khi tạo shipment: {ex.Message}");
        }
    }
}