using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.FarmProfile;
using BLL.DTO.User;
using BLL.Helpers.AddressHelper;
using BLL.Helpers.FarmProfiles;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services
{
    public class FarmProfileService : IFarmProfileService
    {
        private readonly IFarmProfileRepository _farmRepo;
        private readonly IMapper _mapper;
        private readonly IAddressRepository _addressRepo;
        
        public FarmProfileService(IFarmProfileRepository farmRepo, IMapper mapper, IAddressRepository addressRepo)
        {
            _farmRepo = farmRepo;
            _mapper = mapper;
            _addressRepo = addressRepo;
        }
        public async Task<FarmProfileResponseDTO> CreateFarmProfileAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
            var address = _mapper.Map<Address>(dto);
            var farmProfile = _mapper.Map<FarmProfile>(dto);
            farmProfile.UserId = currentUserId;
            
            List<Crop> crops = new List<Crop>();
            if(dto.Crops != null)
            {
                var uniqueNames = new Dictionary<string, List<DateOnly>>(StringComparer.OrdinalIgnoreCase);
                foreach (var crop in dto.Crops)
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
                    crops.Add(_mapper.Map<Crop>(crop));
                }
            }
            var createdFarmProfile = await _farmRepo.CreateFarmProfileWithTransactionAsync(farmProfile, address, crops, cancellationToken);
            return _mapper.Map<FarmProfileResponseDTO>(
                await _farmRepo.GetFarmProfileWithRelationByFarmIdAsync(createdFarmProfile.Id, useNoTracking: true,
                    cancellationToken));
        }

        public async Task<FarmProfileResponseDTO> UpdateFarmProfileAsync(ulong id, FarmProfileUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
            var farmProfile = await _farmRepo.GetFarmProfileByFarmIdAsync(id, cancellationToken);
            var address = await _addressRepo.GetAddressByIdAsync(farmProfile.AddressId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ của trang trại.");
            
            AddressHelper.ValidateAddressFields(dto.Province, dto.ProvinceCode, dto.District, dto.DistrictCode, dto.Commune, dto.CommuneCode);
            _mapper.Map(dto, farmProfile);
            _mapper.Map(dto, address);
            
            var updatedFarmProfile = await _farmRepo.UpdateFarmProfileWithTransactionAsync(farmProfile, address, cancellationToken);
            var response = _mapper.Map<FarmProfileResponseDTO>(updatedFarmProfile);
            
            var userDto = _mapper.Map<UserResponseDTO>(updatedFarmProfile.User);
            response.User = userDto;
            if (updatedFarmProfile.Address != null)
            {
                var updatedAddressDto = _mapper.Map<AddressResponseDTO>(updatedFarmProfile.Address);
                response.Address = updatedAddressDto;
            }
            return response;
        }
        
        public async Task<FarmProfileResponseDTO?> GetFarmProfileByFarmIdAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var entity = await _farmRepo.GetFarmProfileWithRelationByFarmIdAsync(id, useNoTracking: true, cancellationToken);
            var response = _mapper.Map<FarmProfileResponseDTO>(entity);
            return response;
        }

        public async Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllFarmProfileByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
        {
            var list = await _farmRepo.GetAllFarmProfilesByUserIdAsync(userId, useNoTracking: true, cancellationToken);
            if (list.Count == 0)
            {
                return Array.Empty<FarmProfileResponseDTO>();
            }
            list = list.OrderByDescending(x => x.UpdatedAt).ToList();
            var responses = _mapper.Map<List<FarmProfileResponseDTO>>(list);
            
            foreach (var i in Enumerable.Range(0, responses.Count))
            {
                var userDto = _mapper.Map<UserResponseDTO>(list[i].User);
                responses[i].User = userDto;
                if (list[i].Address != null)
                {
                    var addressDto = _mapper.Map<AddressResponseDTO>(list[i].Address);
                    responses[i].Address = addressDto;
                }
            }
            return responses.AsReadOnly();
        }
    }
}

