using AutoMapper;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductCertificate;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class ProductCertificateService : IProductCertificateService
    {
        private readonly IProductCertificateRepository _repo;
        private readonly IMapper _mapper;
        private readonly VerdantTechDbContext _db;

        public ProductCertificateService(IProductCertificateRepository repo, IMapper mapper, VerdantTechDbContext db)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
        }

        // ===== READS =====
        public async Task<PagedResponse<ProductCertificateResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetAllAsync(page, pageSize, ct);
            var list = _mapper.Map<List<ProductCertificateResponseDTO>>(items);
            await HydrateFilesAsync(list, ct);
            return ToPaged(list, total, page, pageSize);
        }

        public async Task<PagedResponse<ProductCertificateResponseDTO>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default)
        {
            if (productId == 0) throw new ArgumentException("Không tìm thấy ProductID", nameof(productId));
            var (items, total) = await _repo.GetByProductAsync(productId, page, pageSize, ct);
            var list = _mapper.Map<List<ProductCertificateResponseDTO>>(items);
            await HydrateFilesAsync(list, ct);
            return ToPaged(list, total, page, pageSize);
        }

        public async Task<ProductCertificateResponseDTO?> GetByIdAsync(ulong productCertificateId, CancellationToken ct = default)
        {
            if (productCertificateId == 0) throw new ArgumentException("Không tìm thấy ProductCertificateID", nameof(productCertificateId));
            var entity = await _repo.GetByIdAsync(productCertificateId, ct);
            var dto = _mapper.Map<ProductCertificateResponseDTO?>(entity);
            if (dto == null) return null;

            // nạp Files cho 1 item
            dto.Files = await _db.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && m.OwnerId == dto.Id)
                .OrderBy(m => m.SortOrder)
                .Select(m => new MediaLinkItemDTO
                {
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    Purpose = m.Purpose.ToString(),
                    SortOrder = m.SortOrder
                })
                .ToListAsync(ct);

            return dto;
        }

        // ===== CREATE =====
        public async Task<IReadOnlyList<ProductCertificateResponseDTO>> CreateAsync( ProductCertificateCreateDTO dto, List<MediaLinkItemDTO> addCertificates, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.ProductId == 0) throw new ArgumentException("ProductId không hợp lệ", nameof(dto.ProductId));
            if (addCertificates == null || addCertificates.Count == 0) throw new ArgumentException("Thiếu file chứng chỉ", nameof(addCertificates));

            var results = new List<ProductCertificateResponseDTO>();

            foreach (var f in addCertificates)
            {
                var mediaLinks = new List<MediaLink> {ToMediaLink(f) };
                var entity = new ProductCertificate
                {
                    ProductId = dto.ProductId,
                    CertificationCode = dto.CertificationCode,
                    CertificationName = dto.CertificationName
                };
                var created = await _repo.CreateAsync(entity, mediaLinks, ct);
            var dtoRes = _mapper.Map<ProductCertificateResponseDTO>(created);
            dtoRes.Files = await _db.MediaLinks
            .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && m.OwnerId == created.Id)
            .OrderBy(m => m.SortOrder)
            .Select(m => new MediaLinkItemDTO {
                ImagePublicId = m.ImagePublicId,
                ImageUrl      = m.ImageUrl,
                Purpose       = m.Purpose.ToString(),
                SortOrder     = m.SortOrder
            })
            .ToListAsync(ct);

        results.Add(dtoRes);
    }

    return results;
}


        public async Task<ProductCertificateResponseDTO> UpdateAsync( ProductCertificateUpdateDTO dto, List<MediaLinkItemDTO> addCertificates, List<string> removedCertificates, CancellationToken ct = default)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Id == 0) throw new ArgumentException("Id không hợp lệ", nameof(dto.Id));

            var add = (addCertificates ?? new()).Select(ToMediaLink).ToList();
            var remove = (removedCertificates ?? new List<string>());

            var entity = _mapper.Map<ProductCertificate>(dto);
            var updated = await _repo.UpdateAsync(entity, add, remove, ct);
            var resp = _mapper.Map<ProductCertificateResponseDTO>(updated);
            resp.Files = await _db.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && m.OwnerId == updated.Id)
                .OrderBy(m => m.SortOrder)
                .Select(m => new MediaLinkItemDTO
                {
                    ImagePublicId = m.ImagePublicId,
                    ImageUrl = m.ImageUrl,
                    Purpose = m.Purpose.ToString(),
                    SortOrder = m.SortOrder
                })
                .ToListAsync(ct);
            return resp;
        }

        // ===== STATUS / DELETE =====
        public Task<bool> ChangeStatusAsync(ProductCertificateChangeStatusDTO dto, CancellationToken ct = default)
        {
            if (dto.Id == 0) throw new ArgumentException("Id không hợp lệ");
            if (dto.Status == ProductCertificateStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new ArgumentException("Phải nhập lý do khi từ chối");

            return _repo.ChangeStatusAsync(dto.Id, dto.Status, dto.RejectionReason, dto.VerifiedBy, DateTime.UtcNow, ct);
        }

        public Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);

        // ===== phân trang =====
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
        private async Task HydrateFilesAsync(List<ProductCertificateResponseDTO> list, CancellationToken ct)
        {
            if (list.Count == 0) return;

            var ids = list.Select(x => x.Id).ToList();

            var links = await _db.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && ids.Contains(m.OwnerId))
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    m.OwnerId,
                    Item = new MediaLinkItemDTO
                    {
                        ImagePublicId = m.ImagePublicId,
                        ImageUrl = m.ImageUrl,
                        Purpose = m.Purpose.ToString(),
                        SortOrder = m.SortOrder
                    }
                })
                .ToListAsync(ct);

            var map = links.GroupBy(x => x.OwnerId).ToDictionary(g => g.Key, g => g.Select(z => z.Item).ToList());
            foreach (var dto in list)
            {
                if (map.TryGetValue(dto.Id, out var files))
                    dto.Files = files;
                else
                    dto.Files = new List<MediaLinkItemDTO>();
            }
        }

        private static MediaLink ToMediaLink(MediaLinkItemDTO src) => new()
        {
            OwnerType = MediaOwnerType.ProductCertificates,
            OwnerId = 0,
            ImagePublicId = src.ImagePublicId,
            ImageUrl = src.ImageUrl,
            Purpose =MediaPurpose.CertificatePdf,
            SortOrder = src.SortOrder == 0 ? 1 : src.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
