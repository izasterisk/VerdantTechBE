using BLL.DTO.Address;
using BLL.Interfaces.Infrastructure;
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
        _baseUrl = Environment.GetEnvironmentVariable("GOSHIP_MANAGE_API_ENDPOINT_V1") ?? "https://api.goship.io/api/v1";
        _bearerToken = Environment.GetEnvironmentVariable("GOSHIP_TOKEN_V1") ?? throw new InvalidOperationException("GOSHIP_TOKEN_V1 không được cấu hình trong .env file");
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
            var url = $"{_baseUrl}/cities?limit=-1&sort=name:1";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var dataElement = AddressApiHelpers.ParseGoshipResponse(jsonContent, "tỉnh/thành phố");
            
            var provinces = new List<CourierProvinceResponseDTO>();
            foreach (var item in dataElement.EnumerateArray())
            {
                provinces.Add(new CourierProvinceResponseDTO
                {
                    ProvinceCode = item.GetProperty("code").GetString() ?? string.Empty,
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                });
            }
            
            return provinces;
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "tỉnh/thành phố");
            throw; 
        }
    }

    public async Task<List<CourierDistrictResponseDTO>> GoshipGetDistrictsAsync(string cityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/districts?limit=-1&sort=name:1";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Validate JSON content
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new InvalidOperationException($"Response rỗng khi lấy dữ liệu quận/huyện (ProvinceCode: {cityId}).");
            }
            
            if (jsonContent.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException($"API endpoint không khả dụng hoặc ProvinceCode {cityId} không đúng.");
            }
            
            // Parse and filter districts using streaming approach
            return AddressApiHelpers.ParseAndFilterDistricts(jsonContent, cityId);
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "quận/huyện", cityId);
            throw; 
        }
    }

    public async Task<List<CourierCommuneResponseDTO>> GoshipGetCommunesAsync(string districtId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/wards?district_code={districtId}&limit=-1&sort=name:1";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var dataElement = AddressApiHelpers.ParseGoshipResponse(jsonContent, "phường/xã", districtId);
            
            var communes = new List<CourierCommuneResponseDTO>();
            foreach (var item in dataElement.EnumerateArray())
            {
                communes.Add(new CourierCommuneResponseDTO
                {
                    CommuneCode = item.GetProperty("id").GetInt32().ToString(),
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                });
            }
            
            return communes;
        }
        catch (Exception ex)
        {
            AddressApiHelpers.HandleApiException(ex, "phường/xã", districtId);
            throw; 
        }
    }
}