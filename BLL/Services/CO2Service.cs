using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.Helpers.CO2;
using DAL.IRepository;

namespace BLL.Services;

public class CO2Service : ICO2Service
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly ISoilGridsApiClient _soilGridsApiClient;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly IEnvironmentalDataRepository _environmentalDataRepository;

    public CO2Service(
        IFarmProfileRepository farmProfileRepository,
        ISoilGridsApiClient soilGridsApiClient,
        IWeatherApiClient weatherApiClient,
        IEnvironmentalDataRepository environmentalDataRepository)
    {
        _farmProfileRepository = farmProfileRepository;
        _soilGridsApiClient = soilGridsApiClient;
        _weatherApiClient = weatherApiClient;
        _environmentalDataRepository = environmentalDataRepository;
    }

    public async Task SaveSoilDataByFarmIdAsync(ulong farmId, ulong customerId, DateOnly measurementStartDate, DateOnly measurementEndDate, CancellationToken cancellationToken = default)
    {
        // Check if data already exists for this farm and date range
        var dataExists = await _environmentalDataRepository.GetByFarmAndDateRangeAsync(farmId, measurementStartDate, measurementEndDate, cancellationToken);
        if (dataExists)
        {
            throw new InvalidOperationException("Dữ liệu đất cho khoảng thời gian này đã tồn tại!");
        }
        
        // Get farm profile with address coordinates
        var farmProfile = await _farmProfileRepository.GetCoordinateByFarmIdAsync(farmId, true, cancellationToken);
        
        if (farmProfile?.Address?.Latitude == null || farmProfile.Address.Longitude == null)
        {
            throw new InvalidOperationException("Nông trại chưa có tọa độ địa lý, không thể lấy dữ liệu đất!");
        }

        try
        {
            // Get both soil data and weather data concurrently
            var soilDataTask = _soilGridsApiClient.GetSoilDataAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                cancellationToken);
            
            var weatherDataTask = _weatherApiClient.GetHistoricalWeatherDataAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                measurementStartDate, 
                measurementEndDate, 
                cancellationToken);

            // Wait for both tasks to complete
            var rawSoilData = await soilDataTask;
            var (precipitationData, et0Data) = await weatherDataTask;

            // Calculate weighted averages for soil data
            var sandAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.SandLayers[0], rawSoilData.SandLayers[1], rawSoilData.SandLayers[2]);
            var siltAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.SiltLayers[0], rawSoilData.SiltLayers[1], rawSoilData.SiltLayers[2]);
            var clayAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.ClayLayers[0], rawSoilData.ClayLayers[1], rawSoilData.ClayLayers[2]);
            var phAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.PhLayers[0], rawSoilData.PhLayers[1], rawSoilData.PhLayers[2]);

            // Calculate weather averages
            var (precipitationAvg, et0Avg) = CalculationHelper.CalculateHistoricalWeatherAverages(precipitationData, et0Data);

            // Create environmental data with both soil and weather data
            await _environmentalDataRepository.CreateEnvironmentalDataWithSoilAndWeatherDataAsync(
                farmId, customerId, measurementStartDate, measurementEndDate,
                sandAverage, siltAverage, clayAverage, phAverage,
                precipitationAvg, et0Avg,
                "Dữ liệu từ SoilGrids API và OpenMeteo Archive API", cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể lấy dữ liệu đất và thời tiết: {ex.Message}");
        }
    }
}