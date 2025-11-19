using BLL.DTO.Crop;
using BLL.DTO.FarmProfile;
using BLL.IService;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Service
{
    public class CropService : ICropService
    {
        private readonly ICropRepository _repo;

        public CropService(ICropRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CropResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var data = await _repo.GetAllAsync(page, pageSize, ct);

            return data.Select(x => new CropResponseDTO
            {
                Id = x.Id,
                FarmProfileId = x.FarmProfileId,
                CropName = x.CropName,
                PlantingDate = x.PlantingDate,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            });
        }

        public async Task<CropResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var x = await _repo.GetByIdAsync(id, ct);
            if (x == null) return null;

            return new CropResponseDTO
            {
                Id = x.Id,
                FarmProfileId = x.FarmProfileId,
                CropName = x.CropName,
                PlantingDate = x.PlantingDate,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            };
        }

        public async Task<IEnumerable<CropResponseDTO>> CreateAsync(CropCreateDTO dto, CancellationToken ct = default)
        {
            var createdList = new List<CropResponseDTO>();

            foreach (var item in dto.Crops)
            {
                var crop = new Crop
                {
                    FarmProfileId = dto.FarmProfileId,
                    CropName = item.CropName,
                    PlantingDate = item.PlantingDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(crop, ct);

                createdList.Add(new CropResponseDTO
                {
                    Id = crop.Id,
                    FarmProfileId = crop.FarmProfileId,
                    CropName = crop.CropName,
                    PlantingDate = crop.PlantingDate,
                    IsActive = crop.IsActive,
                    CreatedAt = crop.CreatedAt,
                    UpdatedAt = crop.UpdatedAt
                });
            }

            return createdList;
        }

        public async Task<bool> UpdateAsync(ulong id, CropUpdateDTO dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return false;

            entity.CropName = dto.CropName;
            entity.PlantingDate = dto.PlantingDate;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity, ct);
            return true;
        }

        public Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default)
        {
            return _repo.SoftDeleteAsync(id, ct);
        }

        public Task<bool> HardDeleteAsync(ulong id, CancellationToken ct = default)
        {
            return _repo.HardDeleteAsync(id, ct);
        }
    }
}
