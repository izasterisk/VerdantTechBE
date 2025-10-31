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
            var ratesElement = GoshipCourierApiHelpers.ParseFeesListResponse(responseContent);
            
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
                    AvgTimeDeliveryFormat = rate.Report.AvgTimeDeliveryFormat ?? ""
                } : null
            }).ToList();
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "fees-list");
            throw; 
        }
    }
    
    public async Task<string> CreateShipmentAsync(UserResponseDTO from, UserResponseDTO to,
        int codAmount, int length, int width, int height, int weight, int amount,
        int payer, int priceTableId, string metadata, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/shipments";
            // Lấy địa chỉ đầu tiên từ UserResponseDTO
            var addressFrom = from.Address.FirstOrDefault() 
                ?? throw new InvalidOperationException("Người gửi không có địa chỉ.");
            var addressTo = to.Address.FirstOrDefault() 
                ?? throw new InvalidOperationException("Người nhận không có địa chỉ.");
            // Parse CommuneCode sang int cho ward_id
            var wardIdFrom = GoshipCourierApiHelpers.ParseCommuneCodeToWardId(addressFrom.CommuneCode, "người gửi");
            var wardIdTo = GoshipCourierApiHelpers.ParseCommuneCodeToWardId(addressTo.CommuneCode, "người nhận");
            
            var requestBody = new
            {
                address_from = new
                {
                    city_code = addressFrom.ProvinceCode ?? throw new InvalidOperationException("Địa chỉ người gửi không có mã tỉnh/thành phố."),
                    district_code = addressFrom.DistrictCode ?? throw new InvalidOperationException("Địa chỉ người gửi không có mã quận/huyện."),
                    ward_id = wardIdFrom,
                    street = addressFrom.LocationAddress ?? throw new InvalidOperationException("Địa chỉ người gửi không có thông tin đường.")
                },
                address_to = new
                {
                    city_code = addressTo.ProvinceCode ?? throw new InvalidOperationException("Địa chỉ người nhận không có mã tỉnh/thành phố."),
                    district_code = addressTo.DistrictCode ?? throw new InvalidOperationException("Địa chỉ người nhận không có mã quận/huyện."),
                    ward_id = wardIdTo,
                    street = addressTo.LocationAddress ?? throw new InvalidOperationException("Địa chỉ người nhận không có thông tin đường.")
                },
                picker = new
                {
                    name = from.FullName,
                    phone = from.PhoneNumber ?? throw new InvalidOperationException("Người gửi không có số điện thoại.")
                },
                receiver = new
                {
                    name = to.FullName,
                    phone = to.PhoneNumber ?? throw new InvalidOperationException("Người nhận không có số điện thoại.")
                },
                parcel = new
                {
                    name = "Nông nghiệp",
                    quantity = 1,
                    cod_amount = codAmount,
                    length = length,
                    width = width,
                    height = height,
                    weight = weight,
                    amount = amount
                },
                metadata = metadata,
                payer = payer,
                price_table_id = priceTableId,
                note_code = "CHOXEMHANGKHONGTHU",
                client_id = 4067
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            return GoshipCourierApiHelpers.ParseShipmentResponse(responseContent);
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