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
            var dataElement = AddressApiHelpers.ParseGoshipResponse(jsonContent, "tỉnh/thành phố");
            
            var provinces = new List<CourierProvinceResponseDTO>();
            foreach (var item in dataElement.EnumerateArray())
            {
                provinces.Add(new CourierProvinceResponseDTO
                {
                    ProvinceCode = item.GetProperty("id").GetString() ?? string.Empty,
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
            var url = $"{_baseUrl}/cities/{cityId}/districts";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var dataElement = AddressApiHelpers.ParseGoshipResponse(jsonContent, "quận/huyện", cityId);
            
            var districts = new List<CourierDistrictResponseDTO>();
            foreach (var item in dataElement.EnumerateArray())
            {
                districts.Add(new CourierDistrictResponseDTO
                {
                    DistrictCode = item.GetProperty("id").GetString() ?? string.Empty,
                    Name = item.GetProperty("name").GetString() ?? string.Empty
                });
            }
            
            return districts;
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
            var url = $"{_baseUrl}/districts/{districtId}/wards";
            
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