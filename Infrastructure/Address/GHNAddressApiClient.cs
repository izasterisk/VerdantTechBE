using System.Text.Json;
using BLL.DTO.Address;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Address.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Address;

public class GHNAddressApiClient : IGHNAddressApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _token;
    private readonly int _timeoutSeconds;

    public GHNAddressApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = Environment.GetEnvironmentVariable("GHN_PRODUCTION_URL") ?? "https://online-gateway.ghn.vn/shiip/public-api/master-data";
        _token = Environment.GetEnvironmentVariable("GHN_PRODUCTION_TOKEN") ?? throw new InvalidOperationException("GHN_PRODUCTION_TOKEN không được cấu hình trong .env file");
        _timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("TIME_OUT_SECONDS") ?? "10");
        
        // Configure HttpClient timeout and Token header (không dùng Bearer)
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("Token", _token);
        
    }

    public async Task<List<CourierProvinceResponseDTO>> GHNGetProvincesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/province";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // Use helper to parse and validate response
            var provinces = AddressApiHelpers.ParseGhnResponse(jsonContent, dataElement =>
            {
                AddressApiHelpers.ValidateArrayData(dataElement, "tỉnh/thành phố");
                var provinceList = new List<Province>();
                foreach (var item in dataElement.EnumerateArray())
                {
                    var province = new Province
                    {
                        ProvinceCode = item.GetProperty("ProvinceID").GetInt32(),
                        Name = item.GetProperty("ProvinceName").GetString() ?? string.Empty
                    };
                    provinceList.Add(province);
                }
                return provinceList;
            }, "tỉnh/thành phố");
            
            // Manual mapping to DTO
            return provinces.Select(province => new CourierProvinceResponseDTO
            {
                ProvinceId = province.ProvinceCode,
                Name = province.Name
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
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách tỉnh/thành phố: {ex.Message}");
        }
    }

    public async Task<List<CourierDistrictResponseDTO>> GHNGetDistrictsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/district";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            var districts = AddressApiHelpers.ParseGhnResponse(jsonContent, dataElement =>
            {
                AddressApiHelpers.ValidateArrayData(dataElement, "quận/huyện");
                
                var districtList = new List<District>();
                foreach (var item in dataElement.EnumerateArray())
                {
                    var district = new District
                    {
                        DistrictCode = item.GetProperty("DistrictID").GetInt32(),
                        ProvinceCode = item.GetProperty("ProvinceID").GetInt32(),
                        Name = item.GetProperty("DistrictName").GetString() ?? string.Empty
                    };
                    districtList.Add(district);
                }
                return districtList;
            }, "quận/huyện");
            
            // Manual mapping to DTO
            return districts.Select(district => new CourierDistrictResponseDTO
            {
                DistrictId = district.DistrictCode,
                ProvinceId = district.ProvinceCode,
                Name = district.Name
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
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách quận/huyện: {ex.Message}");
        }
    }

    public async Task<List<CourierCommuneResponseDTO>> GHNGetCommunesAsync(int districtId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/ward?district_id={districtId}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Use helper to parse and validate response
            var communes = AddressApiHelpers.ParseGhnResponse(jsonContent, dataElement =>
            {
                AddressApiHelpers.ValidateArrayData(dataElement, "phường/xã");
                
                var communeList = new List<Commune>();
                foreach (var item in dataElement.EnumerateArray())
                {
                    var commune = new Commune
                    {
                        CommuneCode = item.GetProperty("WardCode").GetString() ?? string.Empty,
                        DistrictCode = item.GetProperty("DistrictID").GetInt32(),
                        Name = item.GetProperty("WardName").GetString() ?? string.Empty
                    };
                    communeList.Add(commune);
                }
                return communeList;
            }, $"phường/xã cho quận/huyện {districtId}");
            
            // Manual mapping to DTO
            return communes.Select(commune => new CourierCommuneResponseDTO
            {
                CommuneCode = commune.CommuneCode,
                DistrictId = commune.DistrictCode,
                Name = commune.Name
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
            throw new InvalidOperationException($"Lỗi không xác định khi lấy danh sách phường/xã cho quận/huyện {districtId}: {ex.Message}");
        }
    }
}
