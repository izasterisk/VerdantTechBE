// BLL/Services/ProductService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.Product;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _mapper;
        private readonly VerdantTechDbContext _db;

        public ProductService(IProductRepository repo, IMapper mapper, VerdantTechDbContext db)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
        }

        // ========== READS (Paged) ==========
        public async Task<PagedResponse<ProductListItemDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetAllProductAsync(page, pageSize, ct);
            var list = _mapper.Map<List<ProductListItemDTO>>(items);

            // hydrate ảnh (thumbnail đầu tiên) nếu muốn
            await HydrateImagesAsync(list.Select(x => x.Id).ToList(), list, ct);

            return ToPaged(list, total, page, pageSize);
        }

        public async Task<PagedResponse<ProductListItemDTO>> GetByCategoryAsync(ulong categoryId, int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetAllProductByCategoryIdAsync(categoryId, page, pageSize, ct);
            var list = _mapper.Map<List<ProductListItemDTO>>(items);
            await HydrateImagesAsync(list.Select(x => x.Id).ToList(), list, ct);
            return ToPaged(list, total, page, pageSize);
        }

        public async Task<PagedResponse<ProductListItemDTO>> GetByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetAllProductByVendorIdAsync(vendorId, page, pageSize, ct);
            var list = _mapper.Map<List<ProductListItemDTO>>(items);
            await HydrateImagesAsync(list.Select(x => x.Id).ToList(), list, ct);
            return ToPaged(list, total, page, pageSize);
        }

        public async Task<ProductResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _repo.GetProductByIdAsync(id, useNoTracking: true, ct);
            if (entity is null) return null;

            var dto = _mapper.Map<ProductResponseDTO>(entity);
            // Chuyển rating int? -> string (DTO đang là string?)
            dto.EnergyEfficiencyRating = entity.EnergyEfficiencyRating?.ToString();

            // Nạp toàn bộ images
            dto.Images = await LoadImagesAsDtoAsync(id, ct);
            return dto;
        }

        // ========== UPDATE (base fields + images add/remove) ==========
        public async Task<ProductResponseDTO> UpdateAsync(
            ulong id,
            ProductUpdateDTO dto,
            List<MediaLinkItemDTO> addImages,
            List<string> removeImagePublicIds,
            CancellationToken ct = default)
        {
            var entity = await _repo.GetProductByIdAsync(id, useNoTracking: false, ct)
                         ?? throw new KeyNotFoundException("Product không tồn tại.");

            // Update base fields
            entity.CategoryId = dto.CategoryId;
            entity.VendorId = dto.VendorId;
            entity.ProductCode = dto.ProductCode;
            entity.ProductName = dto.ProductName;
            entity.Description = dto.Description;
            entity.UnitPrice = dto.UnitPrice;
            entity.CommissionRate = dto.CommissionRate;
            entity.DiscountPercentage = dto.DiscountPercentage;
            entity.EnergyEfficiencyRating = ParseNullableInt(dto.EnergyEfficiencyRating);
            entity.Specifications = dto.Specifications ?? entity.Specifications;
            entity.ManualUrls = dto.ManualUrls ?? entity.ManualUrls;
            entity.PublicUrl = dto.PublicUrl ?? entity.PublicUrl;
            entity.WarrantyMonths = dto.WarrantyMonths;
            entity.StockQuantity = dto.StockQuantity;
            entity.WeightKg = dto.WeightKg;

            // Dimensions từ DTO (Width/Height/Length) -> dict
            if (dto.DimensionsCm != null)
            {
                entity.DimensionsCm = new Dictionary<string, decimal>
                {
                    ["width"] = dto.DimensionsCm.Width,
                    ["height"] = dto.DimensionsCm.Height,
                    ["length"] = dto.DimensionsCm.Length
                };
            }

            entity.IsActive = dto.IsActive;
            entity.ViewCount = dto.ViewCount;
            entity.SoldCount = dto.SoldCount;
            entity.RatingAverage = dto.RatingAverage;
            entity.UpdatedAt = DateTime.UtcNow;

            // Lưu entity
            entity = await _repo.UpdateProductAsync(entity, ct);

            // Xoá ảnh theo publicId
            if (removeImagePublicIds is { Count: > 0 })
            {
                var toRemove = await _db.MediaLinks
                    .Where(m => m.OwnerType == MediaOwnerType.Products
                             && m.OwnerId == entity.Id
                             && m.ImagePublicId != null
                             && removeImagePublicIds.Contains(m.ImagePublicId))
                    .ToListAsync(ct);

                if (toRemove.Count > 0)
                {
                    _db.MediaLinks.RemoveRange(toRemove);
                    await _db.SaveChangesAsync(ct);
                }
            }

            // Thêm ảnh mới
            if (addImages is { Count: > 0 })
            {
                await SaveImagesAsync(entity.Id, addImages, ct);
            }

            // Trả về DTO đầy đủ
            var result = _mapper.Map<ProductResponseDTO>(entity);
            result.EnergyEfficiencyRating = entity.EnergyEfficiencyRating?.ToString();
            result.Images = await LoadImagesAsDtoAsync(entity.Id, ct);
            return result;
        }

        // ========== UPDATE EMISSION (CommissionRate) ==========
        public Task<bool> UpdateEmissionAsync(ProductUpdateEmissionDTO dto, CancellationToken ct = default)
            => _repo.UpdateEmissionAsync(dto.Id, dto.CommissionRate, ct);

        // ========== DELETE ==========
        public Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);

        // ========== Helpers ==========
        private static PagedResponse<T> ToPaged<T>(List<T> items, int totalRecords, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            return new PagedResponse<T>
            {
                Data = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        private static int? ParseNullableInt(string? s)
            => int.TryParse(s, out var v) ? v : null;

        private async Task<List<MediaLinkItemDTO>> LoadImagesAsDtoAsync(ulong productId, CancellationToken ct)
        {
            var medias = await _db.MediaLinks.AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.Products && m.OwnerId == productId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            return medias.Select(m => new MediaLinkItemDTO
            {
                Id = m.Id,
                ImagePublicId = m.ImagePublicId,
                ImageUrl = m.ImageUrl,
                Purpose = m.Purpose.ToString().ToLowerInvariant(),
                SortOrder = m.SortOrder
            }).ToList();
        }

        private async Task SaveImagesAsync(ulong productId, IReadOnlyList<MediaLinkItemDTO> addImages, CancellationToken ct)
        {
            var start = await _db.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.Products && m.OwnerId == productId)
                .Select(m => (int?)m.SortOrder)
                .MaxAsync(ct) ?? 0;

            var now = DateTime.UtcNow;
            var sort = start;
            var list = addImages.Select(i => new MediaLink
            {
                OwnerType = MediaOwnerType.Products,
                OwnerId = productId,
                ImagePublicId = i.ImagePublicId,
                ImageUrl = i.ImageUrl,
                Purpose = ParsePurpose(i.Purpose),
                SortOrder = ++sort,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            _db.MediaLinks.AddRange(list);
            await _db.SaveChangesAsync(ct);
        }

        private static MediaPurpose ParsePurpose(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return MediaPurpose.None;
            return Enum.TryParse<MediaPurpose>(s, true, out var p) ? p : MediaPurpose.None;
        }

        /// <summary>
        /// Gắn thumbnail (ảnh sort nhỏ nhất) vào ProductListItemDTO
        /// </summary>
        private async Task HydrateImagesAsync(List<ulong> ids, List<ProductListItemDTO> rows, CancellationToken ct)
        {
            if (ids.Count == 0) return;

            // ✅ CÁCH 1: Lấy tất cả images rồi group ở client-side (đơn giản, dễ debug)
            var allImages = await _db.MediaLinks.AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.Products && ids.Contains(m.OwnerId))
                .OrderBy(m => m.OwnerId)
                .ThenBy(m => m.SortOrder)
                .ToListAsync(ct);

            // Group và lấy ảnh đầu tiên cho mỗi product (client-side)
            var firstImagesByProduct = allImages
                .GroupBy(m => m.OwnerId)
                .Select(g => g.First())
                .ToList();

            var byId = rows.ToDictionary(x => x.Id);

            foreach (var img in firstImagesByProduct)
            {
                if (!byId.TryGetValue(img.OwnerId, out var row)) continue;

                // ✅ CHỈ GÁN 1 ẢNH VÀO MẢNG
                row.Images = new List<MediaLinkItemDTO>
        {
            new MediaLinkItemDTO
            {
                Id = img.Id,
                ImagePublicId = img.ImagePublicId,
                ImageUrl = img.ImageUrl,
                Purpose = img.Purpose.ToString().ToLowerInvariant(),
                SortOrder = img.SortOrder
            }
        };
            }
        }
    
    }
}
