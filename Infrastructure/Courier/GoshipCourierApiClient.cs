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
        _baseUrl = Environment.GetEnvironmentVariable("GOSHIP_MANAGE_API_ENDPOINT_V1") ?? "https://api.goship.io/api/v1";
        _bearerToken = Environment.GetEnvironmentVariable("GOSHIP_TOKEN_V1") ?? throw new InvalidOperationException("GOSHIP_TOKEN_V1 không được cấu hình trong .env file");
        _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("TIME_OUT_SECONDS") ?? "10");
        
        // Configure HttpClient timeout and Bearer token
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);
    }
    
    public async Task<List<RateResponseDTO>> GetRatesAsync(string fromDistrictCode, string fromCityCode, string toDistrictCode, 
        string toCityCode, int codAmount, int width, int height, int length, int weight, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/fees-list";
            
            var requestBody = new
            {
                address_from = new
                {
                    city_code = fromCityCode,
                    district_code = fromDistrictCode
                },
                address_to = new
                {
                    city_code = toCityCode,
                    district_code = toDistrictCode
                },
                parcel = new
                {
                    cod_amount = codAmount,
                    length = length,
                    width = width,
                    height = height,
                    weight = weight
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var dataElement = AddressApiHelpers.ParseGoshipResponse(responseContent, "fees-list");
            
            // V1 API có nested structure: data.rates
            if (!dataElement.TryGetProperty("rates", out var ratesElement))
            {
                throw new InvalidOperationException("Response không có trường 'rates' trong data.");
            }
            
            var rates = JsonSerializer.Deserialize<List<RateResponse>>(
                ratesElement.GetRawText(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            
            if (rates == null) return new List<RateResponseDTO>();
            
            return rates.Select(rate => new RateResponseDTO
            {
                ServiceId = rate.ServiceId,
                ServiceMappingId = rate.ServiceMappingId,
                CarrierId = rate.CarrierId,
                CarrierName = rate.CarrierName,
                CarrierShortName = rate.CarrierShortName,
                CarrierLogo = rate.CarrierLogo,
                CarrierNote = rate.CarrierNote,
                Service = rate.Service,
                ExpectedTxt = rate.ExpectedTxt,
                ServiceDescription = rate.ServiceDescription,
                HourApplyTxt = rate.HourApplyTxt,
                IsApplyOnly = rate.IsApplyOnly,
                Parent = rate.Parent != null ? new RateParentDTO
                {
                    Id = rate.Parent.Id,
                    Name = rate.Parent.Name,
                    ShortName = rate.Parent.ShortName,
                    Icon = rate.Parent.Icon,
                    Priority = rate.Parent.Priority,
                    Description = rate.Parent.Description
                } : null,
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
                ReturnFee = rate.ReturnFee,
                Report = rate.Report != null ? new RateReportDTO
                {
                    ScorePercent = rate.Report.ScorePercent,
                    SuccessPercent = rate.Report.SuccessPercent,
                    ReturnPercent = rate.Report.ReturnPercent,
                    AvgTimeDelivery = rate.Report.AvgTimeDelivery,
                    AvgTimeDeliveryFormat = rate.Report.AvgTimeDeliveryFormat
                } : null
            }).ToList();
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "fees-list");
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