using BLL.DTO.ForumCategory;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ForumCategoryService : IForumCategoryService
    {
        private readonly IForumCategoryRepository _repo;

        public ForumCategoryService(IForumCategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ForumCategoryResponseDto>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var list = await _repo.GetAllAsync(page, pageSize, cancellationToken);

            return list.Select(x => new ForumCategoryResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            });
        }

        public async Task<ForumCategoryResponseDto?> GetByIdAsync(
            ulong id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _repo.GetByIdAsync(id, cancellationToken);
            if (entity is null) return null;

            return new ForumCategoryResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public async Task CreateAsync(
            ForumCategoryCreateDTO dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new ForumCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(
            ulong id,
            ForumCategoryUpdateDTO dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _repo.GetByIdAsync(id, cancellationToken);
            if (entity is null) return;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity, cancellationToken);
        }

        public async Task DeleteAsync(
            ulong id,
            CancellationToken cancellationToken = default)
        {
            await _repo.DeleteAsync(id, cancellationToken);
        }
    }
}
