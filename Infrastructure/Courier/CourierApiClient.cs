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
    private readonly string _bearerToken;
    private readonly int _timeoutSeconds;

    public CourierApiClient(HttpClient httpClient, IConfiguration configuration)
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

    public async Task<List<CourierProvinceResponseDTO>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/cities";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Check if response is HTML (error page) instead of JSON
            if (jsonContent.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException("API endpoint không khả dụng hoặc URL không đúng.");
            }
            
            CitiesResponse? citiesResponse;
            try
            {
                citiesResponse = JsonSerializer.Deserialize<CitiesResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            
            if (citiesResponse?.Data == null)
            {
                throw new InvalidOperationException("Không thể lấy dữ liệu tỉnh/thành phố từ GoShip.");
            }
            
            // Manual mapping to DTO
            return citiesResponse.Data.Select(city => new CourierProvinceResponseDTO
            {
                Id = city.Id,
                Name = city.Name
            }).ToList();
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GoShip: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách tỉnh/thành phố: {ex.Message}");
        }
    }

    public async Task<List<CourierDistrictResponseDTO>> GetDistrictsAsync(string cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/cities/{cityId}/districts";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Check if response is HTML (error page) instead of JSON
            if (jsonContent.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException($"API endpoint không khả dụng hoặc mã thành phố {cityId} không đúng.");
            }
            
            DistrictsResponse? districtsResponse;
            try
            {
                districtsResponse = JsonSerializer.Deserialize<DistrictsResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            
            if (districtsResponse?.Data == null)
            {
                throw new InvalidOperationException($"Không thể lấy dữ liệu quận/huyện cho thành phố {cityId} từ GoShip.");
            }
            
            // Manual mapping to DTO
            return districtsResponse.Data.Select(district => new CourierDistrictResponseDTO
            {
                Id = district.Id,
                Name = district.Name
            }).ToList();
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GoShip: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách quận/huyện cho thành phố {cityId}: {ex.Message}");
        }
    }

    public async Task<List<CourierWardResponseDTO>> GetWardsAsync(string districtId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/districts/{districtId}/wards";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Check if response is HTML (error page) instead of JSON
            if (jsonContent.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException($"API endpoint không khả dụng hoặc mã quận/huyện {districtId} không đúng.");
            }
            
            WardsResponse? wardsResponse;
            try
            {
                wardsResponse = JsonSerializer.Deserialize<WardsResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            
            if (wardsResponse?.Data == null)
            {
                throw new InvalidOperationException($"Không thể lấy dữ liệu phường/xã cho quận/huyện {districtId} từ GoShip.");
            }
            
            // Manual mapping to DTO
            return wardsResponse.Data.Select(ward => new CourierWardResponseDTO
            {
                Id = ward.Id,
                Name = ward.Name
            }).ToList();
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GoShip: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw custom exceptions
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách phường/xã cho quận/huyện {districtId}: {ex.Message}");
        }
    }

    public async Task<List<RateResponseDTO>> GetRatesAsync(int fromDistrict, int fromCity, int toDistrict, int toCity, 
        decimal cod, decimal amount, decimal width, decimal height, decimal length, decimal weight, 
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
            
            // Check if response is HTML (error page) instead of JSON
            if (responseContent.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException("API endpoint không khả dụng hoặc URL không đúng.");
            }
            
            var ratesResponse = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            var dataElement = ratesResponse?.RootElement.GetProperty("data");
            
            if (dataElement == null || dataElement.Value.ValueKind == JsonValueKind.Null)
            {
                throw new InvalidOperationException("Không thể lấy dữ liệu rates từ GoShip.");
            }
            
            var rates = JsonSerializer.Deserialize<List<RateResponse>>(dataElement.Value.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rates == null) return new List<RateResponseDTO>();
            
            // Manual mapping to DTO
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
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối đến server GoShip: {ex.Message}");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi không xác định khi lấy rates: {ex.Message}");
        }
    }
}