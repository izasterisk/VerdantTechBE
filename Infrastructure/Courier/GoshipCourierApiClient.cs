using System.Text.Json;
using BLL.DTO.Courier;
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
        string toCity, decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight, 
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
}