using AutoMapper;
using BLL.DTO.Address;
using BLL.DTO.FarmProfile;
using BLL.DTO.User;
using BLL.Helpers.AddressHelper;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services
{
    public class FarmProfileService : IFarmProfileService
    {
        private readonly IFarmProfileRepository _farmRepo;
        private readonly IMapper _mapper;
        
        public FarmProfileService(IFarmProfileRepository farmRepo, IMapper mapper)
        {
            _farmRepo = farmRepo;
            _mapper = mapper;
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
                foreach (var crop in dto.Crops)
                {
                    if(crop.PlantingDate > DateOnly.FromDateTime(DateTime.UtcNow))
                    {
                        throw new ArgumentException("Ngày trồng không được lớn hơn ngày hiện tại.");
                    }
                    crops.Add(_mapper.Map<Crop>(crop));
                }
            }

            var createdFarmProfile = await _farmRepo.CreateFarmProfileWithTransactionAsync(farmProfile, address, crops, cancellationToken);
            var response = _mapper.Map<FarmProfileResponseDTO>(createdFarmProfile);
            
            var userDto = _mapper.Map<UserResponseDTO>(createdFarmProfile.User);
            response.User = userDto;
            if (createdFarmProfile.Address != null)
            {
                var addressDto = _mapper.Map<AddressResponseDTO>(createdFarmProfile.Address);
                response.Address = addressDto;
            }
            return response;
        }

        public async Task<FarmProfileResponseDTO> UpdateFarmProfileAsync(ulong id, FarmProfileUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto, $"{nameof(dto)} rỗng.");
            var farmProfile = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: false, cancellationToken);
            if (farmProfile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ trang trại");
            if (farmProfile.Address == null)
                throw new InvalidOperationException("Địa chỉ của trang trại không tồn tại");
            
            AddressHelper.ValidateAddressFields(dto.Province, dto.ProvinceCode, dto.District, dto.DistrictCode, dto.Commune, dto.CommuneCode);
            _mapper.Map(dto, farmProfile);
            _mapper.Map(dto, farmProfile.Address);
            
            if(dto.Crops != null && dto.Crops.Count > 0)
            {
                foreach (var cropDto in dto.Crops)
                {
                    if(cropDto.PlantingDate.HasValue && cropDto.PlantingDate > DateOnly.FromDateTime(DateTime.UtcNow))
                        throw new ArgumentException("Ngày trồng không được lớn hơn ngày hiện tại.");
                    if(!await _farmRepo.ValidateCropBelongToFarm(cropDto.Id, farmProfile.Id, cancellationToken))
                        throw new KeyNotFoundException($"Cây trồng với ID {cropDto.Id} không thuộc về trang trại này.");
                    
                    var existingCrop = farmProfile.Crops?.FirstOrDefault(x => x.Id == cropDto.Id);
                    if(existingCrop == null)
                        throw new KeyNotFoundException($"Cây trồng với ID {cropDto.Id} không thuộc về trang trại này hoặc đã bị xóa.");
                    _mapper.Map(cropDto, existingCrop);
                    existingCrop.UpdatedAt = DateTime.UtcNow;
                }
            }
            
            var updatedFarmProfile = await _farmRepo.UpdateFarmProfileWithTransactionAsync(farmProfile, farmProfile.Address, cancellationToken);
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
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: true, cancellationToken);
            if (entity == null) return null;
            
            var response = _mapper.Map<FarmProfileResponseDTO>(entity);
            
            var userDto = _mapper.Map<UserResponseDTO>(entity.User);
            response.User = userDto;
            if (entity.Address != null)
            {
                var addressDto = _mapper.Map<AddressResponseDTO>(entity.Address);
                response.Address = addressDto;
            }
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

