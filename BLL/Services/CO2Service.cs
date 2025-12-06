using AutoMapper;
using BLL.DTO.CO2;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.Helpers.CO2;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Services;

public class CO2Service : ICO2Service
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly ISoilGridsApiClient _soilGridsApiClient;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly IEnvironmentalDataRepository _environmentalDataRepository;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    public CO2Service(
        IFarmProfileRepository farmProfileRepository,
        ISoilGridsApiClient soilGridsApiClient,
        IWeatherApiClient weatherApiClient,
        IEnvironmentalDataRepository environmentalDataRepository,
        IMemoryCache cache,
        IMapper mapper)
    {
        _farmProfileRepository = farmProfileRepository;
        _soilGridsApiClient = soilGridsApiClient;
        _weatherApiClient = weatherApiClient;
        _environmentalDataRepository = environmentalDataRepository;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<CO2FootprintResponseDTO> CreateCO2FootprintAsync(ulong farmId, CO2FootprintCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if(dto.MeasurementEndDate <= dto.MeasurementStartDate || dto.MeasurementEndDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new InvalidOperationException("Ngày kết thúc đo đạc phải sau ngày bắt đầu đo đạc và nằm trước hôm nay!");
        var dataExists = await _environmentalDataRepository.GetEnvironmentDataByFarmIdAndDateRangeAsync(farmId, dto.MeasurementStartDate, dto.MeasurementEndDate, cancellationToken);
        if (dataExists)
            throw new InvalidOperationException("Dữ liệu CO2 footprint cho khoảng thời gian này đã tồn tại!");
        var farmProfile = await _farmProfileRepository.GetFarmWithAddressByFarmIdAsync(farmId, true, cancellationToken);
        if (farmProfile.Address?.Latitude == null || farmProfile.Address.Longitude == null)
            throw new InvalidOperationException("Nông trại chưa có tọa độ địa lý, không thể lấy dữ liệu đất!");
        try
        {
            // Get soil data - check cache first
            BLL.DTO.Soil.SoilDataResult rawSoilData;
            var soilCacheKey = $"soil:{farmId}";
            if (_cache.TryGetValue(soilCacheKey, out object? cachedSoilObj) &&
                cachedSoilObj?.GetType().GetProperty("Data")?.GetValue(cachedSoilObj) is BLL.DTO.Soil.SoilDataResult cachedSoil)
            {
                rawSoilData = cachedSoil;
            }
            else
            {
                rawSoilData = await _soilGridsApiClient.GetSoilDataAsync(
                    farmProfile.Address.Latitude.Value,
                    farmProfile.Address.Longitude.Value,
                    cancellationToken);
            }

            // Get weather data - check cache and date range
            Dictionary<DateOnly, (decimal?, decimal?)> weatherData;
            var weatherCacheKey = $"weather:historical:{farmId}";
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var oneYearAgo = today.AddYears(-1);
            
            // Check if requested range is within cached range (1 year ago to today)
            var isWithinCachedRange = dto.MeasurementStartDate >= oneYearAgo && dto.MeasurementEndDate <= today;
            
            if (isWithinCachedRange &&
                _cache.TryGetValue(weatherCacheKey, out object? cachedWeatherObj) &&
                cachedWeatherObj?.GetType().GetProperty("Data")?.GetValue(cachedWeatherObj) is Dictionary<DateOnly, (decimal?, decimal?)> cachedWeather)
            {
                // Filter cached data by requested date range
                weatherData = cachedWeather
                    .Where(kvp => kvp.Key >= dto.MeasurementStartDate && kvp.Key <= dto.MeasurementEndDate)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                // Cache miss or out of range - get from API
                weatherData = await _weatherApiClient.GetHistoricalWeatherDataAsync(
                    farmProfile.Address.Latitude.Value,
                    farmProfile.Address.Longitude.Value,
                    dto.MeasurementStartDate,
                    dto.MeasurementEndDate,
                    cancellationToken);
            }

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
            var (precipitationAvg, et0Avg) = CalculationHelper.CalculateHistoricalWeatherAverages(weatherData);
            
            var environmentalData = _mapper.Map<EnvironmentalDatum>(dto);
            environmentalData.FarmProfileId = farmId;
            environmentalData.CustomerId = farmProfile.UserId;
            environmentalData.SandPct = sandAverage;
            environmentalData.SiltPct = siltAverage;
            environmentalData.ClayPct = clayAverage;
            environmentalData.Phh2o = phAverage;
            environmentalData.PrecipitationSum = precipitationAvg;
            environmentalData.Et0FaoEvapotranspiration = et0Avg;
            var energyUsage = _mapper.Map<EnergyUsage>(dto);
            var fertilizer = _mapper.Map<Fertilizer>(dto);
            environmentalData.Co2Footprint = CalculationHelper.ComputeCo2Footprint(environmentalData.SandPct, 
                environmentalData.SiltPct, environmentalData.ClayPct, environmentalData.Phh2o, 
                environmentalData.PrecipitationSum, environmentalData.Et0FaoEvapotranspiration,
                energyUsage.ElectricityKwh, energyUsage.GasolineLiters, energyUsage.DieselLiters,
                fertilizer.OrganicFertilizer, fertilizer.NpkFertilizer, fertilizer.UreaFertilizer, 
                fertilizer.PhosphateFertilizer);
            
            var createdEnvironmentalData =
                await _environmentalDataRepository.CreateEnvironmentalDataWithTransactionAsync(environmentalData,
                    fertilizer, energyUsage, cancellationToken);
            var response = _mapper.Map<CO2FootprintResponseDTO>(createdEnvironmentalData);
            return response;
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

    public async Task<List<CO2FootprintResponseDTO>> GetAllEnvironmentDataByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        var environmentalDataList = await _environmentalDataRepository.GetAllEnvironmentDataByFarmId(farmId, cancellationToken);
        return _mapper.Map<List<CO2FootprintResponseDTO>>(environmentalDataList);
    }

    public async Task<CO2FootprintResponseDTO?> GetEnvironmentDataByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var environmentalData = await _environmentalDataRepository.GetEnvironmentDataById(id, cancellationToken);
        return environmentalData == null ? null : _mapper.Map<CO2FootprintResponseDTO>(environmentalData);
    }

    public async Task<string> DeleteEnvironmentalDataByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var result = await _environmentalDataRepository.DeleteEnvironmentalDataByIdWithTransactionAsync(id, cancellationToken);
        return result ? "Xóa thành công." : "Xóa thất bại.";
    }
}