using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.Helpers.CO2;
using DAL.IRepository;

namespace BLL.Services;

public class CO2Service : ICO2Service
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly ISoilGridsApiClient _soilGridsApiClient;
    private readonly IEnvironmentalDataRepository _environmentalDataRepository;

    public CO2Service(
        IFarmProfileRepository farmProfileRepository,
        ISoilGridsApiClient soilGridsApiClient,
        IEnvironmentalDataRepository environmentalDataRepository)
    {
        _farmProfileRepository = farmProfileRepository;
        _soilGridsApiClient = soilGridsApiClient;
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
            // Get raw soil data from SoilGrids API
            var rawSoilData = await _soilGridsApiClient.GetSoilDataAsync(
                farmProfile.Address.Latitude.Value, 
                farmProfile.Address.Longitude.Value, 
                cancellationToken);

            var sandAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.SandLayers[0], rawSoilData.SandLayers[1], rawSoilData.SandLayers[2]);
            var siltAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.SiltLayers[0], rawSoilData.SiltLayers[1], rawSoilData.SiltLayers[2]);
            var clayAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.ClayLayers[0], rawSoilData.ClayLayers[1], rawSoilData.ClayLayers[2]);
            var phAverage = CalculationHelper.CalculateWeightedAverage(
                rawSoilData.PhLayers[0], rawSoilData.PhLayers[1], rawSoilData.PhLayers[2]);

            // Create environmental data through repository (avoiding direct DAL dependency)
            await _environmentalDataRepository.CreateEnvironmentalDataWithSoilDataAsync(
                farmId, customerId, measurementStartDate, measurementEndDate,
                sandAverage, siltAverage, clayAverage, phAverage,
                "Dữ liệu từ SoilGrids API", cancellationToken);
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể lấy dữ liệu đất từ SoilGrids: {ex.Message}");
        }
    }
}