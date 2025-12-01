using AutoMapper;
using BLL.DTO.Crops;
using BLL.DTO.FarmProfile;
using BLL.Helpers.FarmProfiles;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class CropService : ICropService
{
    private readonly IFarmProfileRepository _farmProfileRepository;
    private readonly ICropRepository _cropRepository;
    private readonly IMapper _mapper;
    
    public CropService(IFarmProfileRepository farmProfileRepository, ICropRepository cropRepository, IMapper mapper)
    {
        _farmProfileRepository = farmProfileRepository;
        _cropRepository = cropRepository;
        _mapper = mapper;
    }

    public async Task<FarmProfileResponseDTO> AddCropsToFarmAsync(ulong farmId, List<CropsCreateDTO> dtos, CancellationToken cancellationToken = default)
    {
        if(dtos.Count == 0)
            throw new ArgumentException("Danh sách cây trồng không được để trống");
        await _cropRepository.IsFarmExistsAsync(farmId, cancellationToken);
        
        var uniqueNames = new Dictionary<string, List<DateOnly>>(StringComparer.OrdinalIgnoreCase);
        var existingCrops = await _cropRepository.GetAllPlantingCropsByFarmIdAsync(farmId, cancellationToken);
        if (existingCrops.Count > 0)
        {
            foreach (var crop in existingCrops)
            {
                if (uniqueNames.TryGetValue(crop.CropName, out var planDates))
                {
                    planDates.Add(crop.PlantingDate);
                }
                else
                {
                    uniqueNames[crop.CropName] = new List<DateOnly> { crop.PlantingDate };
                }
            }
        }
        List<Crop> crops = new List<Crop>();
        foreach (var crop in dtos)
        {
            if (uniqueNames.TryGetValue(crop.CropName, out var planDates))
            {
                if(planDates.Contains(crop.PlantingDate))
                    throw new ArgumentException($"Cây trồng '{crop.CropName}' với ngày trồng '{crop.PlantingDate}' bị trùng lặp.");
                planDates.Add(crop.PlantingDate);
            }
            else
            {
                uniqueNames[crop.CropName] = new List<DateOnly> { crop.PlantingDate };
            }
            FarmProfilesHelper.ValidateCropCombination(crop.PlantingMethod, crop.CropType, crop.FarmingType);
            if(crop.Status is CropStatus.Completed or CropStatus.Deleted or CropStatus.Failed)
                throw new ArgumentException("Trạng thái cây trồng không hợp lệ khi tạo mới.");
            if(crop.PlantingDate > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Ngày trồng không được lớn hơn ngày hiện tại.");
            var cropToCreate = _mapper.Map<Crop>(crop);
            cropToCreate.FarmProfileId = farmId;
            crops.Add(cropToCreate);
        }
        await _cropRepository.CreateBulkCropsAsync(crops, cancellationToken);
        return _mapper.Map<FarmProfileResponseDTO>(
            await _farmProfileRepository.GetFarmProfileWithRelationByFarmIdAsync(farmId, useNoTracking: true,
                cancellationToken));
    }

    public async Task<FarmProfileResponseDTO> UpdateCropsAsync(ulong farmId, List<CropsUpdateDTO> dtos, CancellationToken cancellationToken = default)
    {
        if(dtos.Count == 0)
            throw new ArgumentException("Danh sách cây trồng không được để trống");
        await _cropRepository.IsFarmExistsAsync(farmId, cancellationToken);
        
        var cropsDtos = new Dictionary<ulong, CropsUpdateDTO>(dtos.Count);
        foreach (var dto in dtos)
        {
            if(dto.PlantingDate > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Ngày trồng không được lớn hơn ngày hiện tại.");
            if (cropsDtos.TryGetValue(dto.Id, out var crop))
                throw new ArgumentException($"Phát hiện ID bị trùng lặp trong danh sách cập nhật: {dto.Id}");
            cropsDtos.Add(dto.Id, dto);
        }
        var crops = await _cropRepository.GetAllCropsByFarmIdAsync(farmId, cancellationToken);
        var cropsMap = crops.ToDictionary(c => c.Id);
        var cropsToUpdate = new List<Crop>();
        foreach (var dto in cropsDtos)
        {
            if (!cropsMap.TryGetValue(dto.Key, out var crop))
                throw new KeyNotFoundException($"Cây trồng với ID: {dto.Key} không tồn tại hoặc không phải của trang trại này.");
            _mapper.Map(dto.Value, crop); 
            FarmProfilesHelper.ValidateCropCombination(crop.PlantingMethod, crop.CropType, crop.FarmingType);
            cropsToUpdate.Add(crop);
        }
        await _cropRepository.UpdateBulkCropsAsync(cropsToUpdate, cancellationToken);
        return _mapper.Map<FarmProfileResponseDTO>(
            await _farmProfileRepository.GetFarmProfileWithRelationByFarmIdAsync(farmId, useNoTracking: true,
                cancellationToken));
    }
}