using AutoMapper;
using BLL.DTO.FarmProfile;
using BLL.Helpers;
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

        public FarmProfileService(IFarmProfileRepository farmRepo, IMapper mapper)
        {
            _farmRepo = farmRepo;
            _mapper = mapper;
        }

        public async Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto)
        {
            var entity = _mapper.Map<FarmProfile>(dto);
            entity.UserId = currentUserId;
            entity.Status = FarmProfileStatus.Active;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _farmRepo.CreateAsync(entity);
            return _mapper.Map<FarmProfileResponseDTO>(created);
        }

        public async Task<FarmProfileResponseDTO?> GetAsync(ulong id)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: true);
            return _mapper.Map<FarmProfileResponseDTO>(entity);
        }

        public async Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId)
        {
            var list = await _farmRepo.GetAllFarmProfilesByUserIdAsync(userId, useNoTracking: true);
            if (list == null || !list.Any())
                throw new KeyNotFoundException("Không tìm thấy hồ sơ trang trại nào cho người dùng này.");
            var ordered = list.OrderByDescending(x => x.UpdatedAt).ToList();
            return _mapper.Map<IReadOnlyList<FarmProfileResponseDTO>>(ordered);
        }

        public async Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileUpdateDTO dto)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: false);
            if (entity == null || entity.UserId != currentUserId)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ trang trại hoặc không có quyền truy cập.");
            
            // Xử lý Status nếu có
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                var newStatus = Utils.ParseEnum<FarmProfileStatus>(dto.Status, "trạng thái trang trại");
                entity.Status = newStatus;
            }
            
            // Map những trường có giá trị từ DTO vào entity (đã cấu hình Condition)
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            var updated = await _farmRepo.UpdateAsync(entity);
            return _mapper.Map<FarmProfileResponseDTO>(updated);
        }

        public async Task<bool> DeleteAsync(ulong id, ulong currentUserId)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: false);
            if (entity == null || entity.UserId != currentUserId) return false;
            return await _farmRepo.DeleteAsync(entity);
        }
    }
}

