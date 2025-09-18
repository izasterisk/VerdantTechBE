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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public FarmProfileService(IFarmProfileRepository farmRepo, IUserService userService, IMapper mapper)
        {
            _farmRepo = farmRepo;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken cancellationToken = default)
        {
            var entity = _mapper.Map<FarmProfile>(dto);
            entity.UserId = currentUserId;
            entity.Status = FarmProfileStatus.Active;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var created = await _farmRepo.CreateAsync(entity, cancellationToken);
            var response = _mapper.Map<FarmProfileResponseDTO>(created);
            var user = await _userService.GetUserByIdAsync(created.UserId, cancellationToken);
            if (user != null)
            {
                response.User = user;
            }
            return response;
        }

        public async Task<FarmProfileResponseDTO?> GetAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: true, cancellationToken);
            if (entity == null) return null;
            
            var response = _mapper.Map<FarmProfileResponseDTO>(entity);
            var user = await _userService.GetUserByIdAsync(entity.UserId, cancellationToken);
            if (user != null)
            {
                response.User = user;
            }
            return response;
        }

        public async Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId, CancellationToken cancellationToken = default)
        {
            var list = await _farmRepo.GetAllFarmProfilesByUserIdAsync(userId, useNoTracking: true, cancellationToken);
            if (list.Count == 0)
            {
                return Array.Empty<FarmProfileResponseDTO>();
            }
            list = list.OrderByDescending(x => x.UpdatedAt).ToList();
            var responses = _mapper.Map<List<FarmProfileResponseDTO>>(list);
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                foreach (var response in responses)
                {
                    response.User = user;
                }
            }
            return responses.AsReadOnly();
        }


        public async Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: false, cancellationToken);
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
            var updated = await _farmRepo.UpdateAsync(entity, cancellationToken);
            var response = _mapper.Map<FarmProfileResponseDTO>(updated);
            var user = await _userService.GetUserByIdAsync(updated.UserId, cancellationToken);
            if (user != null)
            {
                response.User = user;
            }
            return response;
        }

        public async Task<bool> DeleteAsync(ulong id, ulong currentUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _farmRepo.GetFarmProfileByFarmIdAsync(id, useNoTracking: false, cancellationToken);
            if (entity == null || entity.UserId != currentUserId) return false;
            return await _farmRepo.DeleteAsync(entity, cancellationToken);
        }
    }
}

