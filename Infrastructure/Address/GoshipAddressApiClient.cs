using System.Text.Json;
using BLL.DTO.Address;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Address;

public class GoshipAddressApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _bearerToken;
    private readonly int _timeoutSeconds;
    
    public GoshipAddressApiClient(HttpClient httpClient, IConfiguration configuration)
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
    
    public async Task<List<CourierProvinceResponseDTO>> GoshipGetProvincesAsync(CancellationToken cancellationToken = default)
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

    public async Task<List<CourierDistrictResponseDTO>> GoshipGetDistrictsAsync(string cityId, CancellationToken cancellationToken = default)
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

    public async Task<List<CourierWardResponseDTO>> GoshipGetCommunesAsync(string districtId, CancellationToken cancellationToken = default)
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
}