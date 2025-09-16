using AutoMapper;
using BLL.DTO.FarmProfile;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace VerdantTech.Application.FarmProfiles
{
    public class FarmProfileService : IFarmProfileService
    {
        private readonly IFarmProfileRepository _farmRepo;
        private readonly IMapper _mapper;

        public FarmProfileService(IFarmProfileRepository farmRepo, IMapper mapper)
        {
            _farmRepo = farmRepo;
            _mapper   = mapper;
        }

        public async Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<FarmProfile>(dto);
            entity.UserId    = currentUserId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _farmRepo.CreateAsync(entity);
            return _mapper.Map<FarmProfileResponseDTO>(created);
        }

        public async Task<FarmProfileResponseDTO?> GetAsync(ulong id, ulong currentUserId, CancellationToken ct = default)
        {
            var entity = await _farmRepo.GetFarmProfileAsync(id, useNoTracking: true);
            if (entity == null || entity.UserId != currentUserId) return null;
            return _mapper.Map<FarmProfileResponseDTO>(entity);
        }

        public async Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId, CancellationToken ct = default)
        {
            var list = await _farmRepo.GetAllFarmProfilesByUserAsync(userId, useNoTracking: true);
            var ordered = list.OrderByDescending(x => x.UpdatedAt).ToList();
            return _mapper.Map<IReadOnlyList<FarmProfileResponseDTO>>(ordered);
        }

        public async Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileResponseDTO dto, CancellationToken ct = default)
        {
            var entity = await _farmRepo.GetFarmProfileAsync(id, useNoTracking: false);
            if (entity == null || entity.UserId != currentUserId)
                throw new KeyNotFoundException("Farm profile not found or access denied.");

            // Map “có kiểm soát” từ ResponseDTO sang entity (vì bạn dùng ResponseDTO cho Update)
            entity.FarmName         = dto.FarmName;
            entity.FarmSizeHectares = dto.FarmSizeHectares;
            entity.LocationAddress  = dto.LocationAddress;
            entity.Province         = dto.Province;
            entity.District         = dto.District;
            entity.IsActive         = dto.IsActive;
            entity.Commune          = dto.Commune;
            entity.PrimaryCrops     = dto.PrimaryCrops;
            entity.UpdatedAt        = DateTime.UtcNow;

            var updated = await _farmRepo.UpdateAsync(entity);
            return _mapper.Map<FarmProfileResponseDTO>(updated);
        }

        public async Task<bool> DeleteAsync(ulong id, ulong currentUserId, CancellationToken ct = default)
        {
            var entity = await _farmRepo.GetFarmProfileAsync(id, useNoTracking: false);
            if (entity == null || entity.UserId != currentUserId) return false;

            return await _farmRepo.DeleteAsync(entity);
        }
    }
}
