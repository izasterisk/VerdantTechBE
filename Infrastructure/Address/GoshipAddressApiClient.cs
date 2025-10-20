using System.Text.Json;
using BLL.DTO.Address;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Address.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Address;

public class GoshipAddressApiClient : IGoshipAddressApiClient
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
            
            // Parse JSON manually
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;
            // Check response structure
            if (!root.TryGetProperty("data", out var dataElement))
            {
                throw new InvalidOperationException("Không thể lấy dữ liệu tỉnh/thành phố từ GoShip.");
            }
            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            // Parse provinces
            var provinces = new List<Province>();
            foreach (var item in dataElement.EnumerateArray())
            {
                var province = new Province
                {
                    ProvinceCode = item.GetProperty("id").GetString() ?? string.Empty,
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                };
                provinces.Add(province);
            }
            // Manual mapping to DTO
            return provinces.Select(province => new CourierProvinceResponseDTO
            {
                ProvinceCode = province.ProvinceCode,
                Name = province.Name
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
            
            // Parse JSON manually
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;
            
            // Check response structure
            if (!root.TryGetProperty("data", out var dataElement))
            {
                throw new InvalidOperationException($"Không thể lấy dữ liệu quận/huyện cho thành phố {cityId} từ GoShip.");
            }
            
            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            
            // Parse districts
            var districts = new List<District>();
            foreach (var item in dataElement.EnumerateArray())
            {
                var district = new District
                {
                    DistrictCode = item.GetProperty("id").GetString() ?? string.Empty,
                    ProvinceCode = cityId,
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                };
                districts.Add(district);
            }
            
            // Manual mapping to DTO
            return districts.Select(district => new CourierDistrictResponseDTO
            {
                DistrictCode = district.DistrictCode,
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

    public async Task<List<CourierCommuneResponseDTO>> GoshipGetCommunesAsync(string districtId, CancellationToken cancellationToken = default)
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
            
            // Parse JSON manually
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;
            
            // Check response structure
            if (!root.TryGetProperty("data", out var dataElement))
            {
                throw new InvalidOperationException($"Không thể lấy dữ liệu phường/xã cho quận/huyện {districtId} từ GoShip.");
            }
            
            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Định dạng dữ liệu từ GoShip không hợp lệ.");
            }
            
            // Parse communes
            var communes = new List<Commune>();
            foreach (var item in dataElement.EnumerateArray())
            {
                var commune = new Commune
                {
                    CommuneCode = item.GetProperty("id").GetInt32().ToString(), // Convert int to string
                    DistrictCode = districtId, 
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                };
                communes.Add(commune);
            }
            
            // Manual mapping to DTO
            return communes.Select(commune => new CourierCommuneResponseDTO
            {
                CommuneCode = commune.CommuneCode,
                DistrictCode = commune.DistrictCode,
                Name = commune.Name
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