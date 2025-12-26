using System.Text.Json;
using BLL.Interfaces.Infrastructure;
using BLL.DTO.Soil;
using Infrastructure.Soil.Models;
using Infrastructure.Weather;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Soil;

public class SoilGridsApiClient : ISoilGridsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _wmsBaseUrl;
    private readonly int _timeoutSeconds;

    public SoilGridsApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["SOIL_GRIDS_URL"] ?? "https://rest.isric.org/soilgrids/v2.0/properties/";
        _wmsBaseUrl = _configuration["SOIL_GRIDS_WMS"] ?? "https://maps.isric.org/mapserv";
        _timeoutSeconds = int.Parse(_configuration["TIME_OUT_SECONDS"] ?? "10");

        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    }

    public async Task<SoilDataResult> GetSoilDataAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        // Retry logic cho WMS API (3 lần)
        try
        {
            return await WeatherHelper.ExecuteWithRetryAsync(async () =>
            {
                // 1) Tính tham số WMS
                var (bbox, i, j) = SoilGridsHelper.ComputeWmsParameters((double)longitude, (double)latitude);

                string[] props = { "sand", "silt", "clay", "phh2o" };
                string[] depths = { "0-5cm", "5-15cm", "15-30cm" };

                var jobs = new List<Task<(string property, string depth, double value, string unit)>>();
                foreach (var p in props)
                foreach (var d in depths)
                {
                    var (map, layer) = SoilGridsHelper.GetWmsMapAndLayer(p, d);
                    var url = SoilGridsHelper.BuildWmsGetFeatureInfoUrl(_wmsBaseUrl, map, layer, bbox, i, j);
                    jobs.Add(FetchWmsValueAsync(url, p, d, cancellationToken));
                }

                var raw = await Task.WhenAll(jobs);

                // 2) Chuẩn hóa đơn vị
                var norm = raw.Select(x =>
                {
                    double v = x.value;
                    if (string.Equals(x.unit, "g/kg", StringComparison.OrdinalIgnoreCase)) v /= 10.0;   // % từ g/kg
                    else if (string.Equals(x.unit, "pH*10", StringComparison.OrdinalIgnoreCase)) v /= 10.0; // pH thực
                    return (x.property, x.depth, v);
                }).ToList();

                // 3) Map sang SoilDataResult
                var result = SoilGridsHelper.FromWmsTriplets(norm);

                // 4) Validate
                SoilGridsHelper.ValidateSoilData(
                    result.SandLayers[0], result.SandLayers[1], result.SandLayers[2],
                    result.SiltLayers[0], result.SiltLayers[1], result.SiltLayers[2],
                    result.ClayLayers[0], result.ClayLayers[1], result.ClayLayers[2],
                    result.PhLayers[0], result.PhLayers[1], result.PhLayers[2]);

                return result;
            });
        }
        catch (Exception wmsEx)
        {
            // Fallback sang CO2 (cũng có retry logic)
            try
            {
                return await GetSoilDataBackupAsync(latitude, longitude, cancellationToken);
            }
            catch (Exception co2Ex)
            {
                throw new InvalidOperationException(
                    "Không thể lấy dữ liệu SoilGrids từ cả WMS (2OC) và CO2 fallback.",
                    new AggregateException(wmsEx, co2Ex));
            }
        }
    }

    private async Task<(string property, string depth, double value, string unit)> FetchWmsValueAsync(
        string url, string property, string depth, CancellationToken ct)
    {
        return await WeatherHelper.ExecuteWithRetryAsync(async () =>
        {
            using var resp = await _httpClient.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            await using var s = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);

            var features = doc.RootElement.GetProperty("features");
            if (features.GetArrayLength() == 0)
                throw new InvalidOperationException("WMS trả về rỗng.");

            var props = features[0].GetProperty("properties");
            double pixel = props.GetProperty("pixel_value").GetDouble();
            string unit = props.TryGetProperty("unit", out var u) ? u.GetString() ?? "" : "";

            return (property, depth, pixel, unit);
        });
    }

    public async Task<SoilDataResult> GetSoilDataBackupAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        return await WeatherHelper.ExecuteWithRetryAsync(async () =>
        {
            try
            {
                var url = SoilGridsHelper.BuildSoilGridsUrl(_baseUrl, latitude, longitude);

                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

                var rawSoilData = JsonSerializer.Deserialize<SoilGridsResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (rawSoilData == null)
                    throw new InvalidOperationException("Không thể giải mã dữ liệu đất từ SoilGrids API");

                var processedData = SoilGridsResponseTransformer.TransformSoilGridsResponse(rawSoilData);

                // Validate
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
        });
    }
}
