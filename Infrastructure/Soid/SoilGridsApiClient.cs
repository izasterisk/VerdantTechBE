using System.Text.Json;
using BLL.Interfaces.Infrastructure;
using Infrastructure.CO2.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.CO2;

public class SoilGridsApiClient : ISoilGridsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly int _timeoutSeconds;

    public SoilGridsApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["SOIL_GRIDS_URL"] ?? "https://rest.isric.org/soilgrids/v2.0/properties/";
        _timeoutSeconds = int.Parse(_configuration["TIME_OUT_SECONDS"] ?? "10");
        
        // Configure HttpClient timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    }

    public async Task<SoilDataResult> GetSoilDataAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = SoilGridsHelper.BuildSoilGridsUrl(_baseUrl, longitude, latitude);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var rawSoilData = JsonSerializer.Deserialize<SoilGridsResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (rawSoilData == null)
            {
                throw new InvalidOperationException("Không thể giải mã dữ liệu đất từ SoilGrids API");
            }
            
            var processedData = SoilGridsResponseTransformer.TransformSoilGridsResponse(rawSoilData);
            
            // Validate that all soil parameters are available
            SoilGridsHelper.ValidateSoilData(
                processedData.SandLayers[0], processedData.SandLayers[1], processedData.SandLayers[2],
                processedData.SiltLayers[0], processedData.SiltLayers[1], processedData.SiltLayers[2],
                processedData.ClayLayers[0], processedData.ClayLayers[1], processedData.ClayLayers[2],
                processedData.PhLayers[0], processedData.PhLayers[1], processedData.PhLayers[2]);
            
            return processedData;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Server SoilGrids hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("Server SoilGrids hiện đang quá tải, vui lòng thử lại sau.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Dữ liệu từ SoilGrids không hợp lệ.");
        }
    }
}