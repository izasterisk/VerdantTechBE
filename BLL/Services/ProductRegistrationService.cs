using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DAL.Repositories;
using System.Globalization;

namespace BLL.Services
{
    public class ProductRegistrationService : IProductRegistrationService
    {
        private readonly IProductRegistrationRepository _repo;
        private readonly IMapper _mapper;
        private readonly VerdantTechDbContext _db;

        public ProductRegistrationService(
            IProductRegistrationRepository repo,
            IMapper mapper,
            VerdantTechDbContext db)
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
        }


        // ================= READS =================

        public async Task<PagedResponse<ProductRegistrationReponseDTO>> GetAllAsync(
            int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetAllAsync(page, pageSize, ct);
            var dtos = _mapper.Map<List<ProductRegistrationReponseDTO>>(items);
            await HydrateMediaAsync(dtos, ct);
            return ToPaged(dtos, total, page, pageSize);
        }

        public async Task<PagedResponse<ProductRegistrationReponseDTO>> GetByVendorAsync(
            ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetByVendorAsync(vendorId, page, pageSize, ct);
            var dtos = _mapper.Map<List<ProductRegistrationReponseDTO>>(items);
            await HydrateMediaAsync(dtos, ct);
            return ToPaged(dtos, total, page, pageSize);
        }

        public async Task<ProductRegistrationReponseDTO?> GetByIdAsync(
            ulong id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return null;

            var dto = _mapper.Map<ProductRegistrationReponseDTO>(entity);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { dto }, ct);
            return dto;
        }


        // ================= CREATE =================

        public async Task<ProductRegistrationReponseDTO> CreateAsync(
            ProductRegistrationCreateDTO dto,
            string? manualUrl,
            string? manualPublicUrl,
            List<MediaLinkItemDTO> addImages,
            List<MediaLinkItemDTO> addCertificates,
            CancellationToken ct = default)
        {
            // validate FK
            if (!await _db.Users.AnyAsync(x => x.Id == dto.VendorId, ct))
                throw new InvalidOperationException("Vendor không tồn tại.");
            if (!await _db.ProductCategories.AnyAsync(x => x.Id == dto.CategoryId, ct))
                throw new InvalidOperationException("Category không tồn tại.");
            if (!await _db.ProductCategories.AnyAsync(x => x.ParentId == null, ct))
                throw new InvalidOperationException("Category không thể là parent.");

            var rating = ParseNullableDecimal(dto.EnergyEfficiencyRating);
            if (rating is < 0 or > 5)
                throw new InvalidOperationException("EnergyEfficiencyRating phải từ 0 đến 5.0");

            var dims = ToDecimalDict(dto.DimensionsCm);
            if (dto.DimensionsCm != null && dims is null)
                throw new InvalidOperationException("Kích thước không hợp lệ.");


            // Map
            var entity = _mapper.Map<ProductRegistration>(dto);

            entity.Specifications = dto.Specifications?.Count > 0
                ? dto.Specifications
                : new Dictionary<string, object>();

            entity.EnergyEfficiencyRating = rating;
            entity.DimensionsCm = dims ?? new Dictionary<string, decimal>();

            entity.Status = ProductRegistrationStatus.Pending;
            entity.ManualUrls = manualUrl;
            entity.PublicUrl = manualPublicUrl;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Map product images
            var productImages = ToMediaLinks(addImages, MediaOwnerType.ProductRegistrations);

            entity = await _repo.CreateAsync(entity, productImages, null, ct);


            // ================= CERTIFICATE CREATE =================
            if (dto.CertificationName != null &&dto.CertificationCode != null)
            {
                // Check số lượng file VS số lượng tên chứng chỉ
                if (addCertificates.Count != dto.CertificationName.Count)
                    throw new InvalidOperationException("Số lượng file chứng chỉ không khớp số lượng tên chứng chỉ.");

                if (dto.CertificationName.Count != dto.CertificationCode.Count)
                    throw new InvalidOperationException("Tên chứng chỉ và mã chứng chỉ không khớp số lượng.");

                for (int i = 0; i < dto.CertificationName.Count; i++)
                {
                    // Create certificate entity
                    var cert = new ProductCertificate
                    {
                        RegistrationId = entity.Id,
                        ProductId = null,
                        CertificationName = dto.CertificationName[i],
                        CertificationCode = dto.CertificationCode[i],
                        Status = ProductCertificateStatus.Pending,
                        UploadedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.ProductCertificates.AddAsync(cert, ct);
                    await _db.SaveChangesAsync(ct);

                    // Attach file
                    var file = addCertificates[i];

                    var media = new MediaLink
                    {
                        OwnerType = MediaOwnerType.ProductCertificates,
                        OwnerId = cert.Id,
                        ImagePublicId = file.ImagePublicId,
                        ImageUrl = file.ImageUrl,
                        Purpose = MediaPurpose.ProductCertificatePdf,
                        SortOrder = i + 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.MediaLinks.AddAsync(media, ct);
                    await _db.SaveChangesAsync(ct);
                }
            }

            var fresh = await _db.ProductRegistrations.AsNoTracking()
                .FirstAsync(x => x.Id == entity.Id, ct);

            var result = _mapper.Map<ProductRegistrationReponseDTO>(fresh);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { result }, ct);

            return result;
        }


        // ================= UPDATE =================

        public async Task<ProductRegistrationReponseDTO> UpdateAsync(
            ProductRegistrationUpdateDTO dto,
            string? manualUrl,
            string? manualPublicUrl,
            List<MediaLinkItemDTO> addImages,
            List<MediaLinkItemDTO> addCertificates,
            List<string> removedImages,
            List<string> removedCertificates,
            CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(dto.Id, ct)
                         ?? throw new KeyNotFoundException("Đơn đăng ký không tồn tại.");

            var rating = ParseNullableDecimal(dto.EnergyEfficiencyRating);
            if (rating is < 0 or > 5)
                throw new InvalidOperationException("EnergyEfficiencyRating 0 - 5.");

            var dims = ToDecimalDict(dto.DimensionsCm);

            // Update core fields
            entity.VendorId = dto.VendorId ?? entity.VendorId;
            entity.CategoryId = dto.CategoryId ?? entity.CategoryId;
            entity.ProposedProductCode = dto.ProposedProductCode;
            entity.ProposedProductName = dto.ProposedProductName;
            entity.Description = dto.Description;
            entity.UnitPrice = dto.UnitPrice ?? entity.UnitPrice;
            entity.EnergyEfficiencyRating = rating;
            entity.Specifications = dto.Specifications ?? entity.Specifications ?? new Dictionary<string, object>();
            entity.DimensionsCm = dims ?? entity.DimensionsCm;

            if (!string.IsNullOrWhiteSpace(manualUrl)) entity.ManualUrls = manualUrl;
            if (!string.IsNullOrWhiteSpace(manualPublicUrl)) entity.PublicUrl = manualPublicUrl;

            entity.UpdatedAt = DateTime.UtcNow;

            var addProductImages = ToMediaLinks(addImages, MediaOwnerType.ProductRegistrations);
            var addCertImages = ToMediaLinks(addCertificates, MediaOwnerType.ProductCertificates);

            entity = await _repo.UpdateAsync(
                entity,
                addProductImages,
                addCertImages,
                removedImages ?? new List<string>(),
                removedCertificates ?? new List<string>(),
                ct);

            // New certificates
            if (addCertificates.Count > 0 &&
    dto.CertificationName != null &&
    dto.CertificationCode != null)
            {
                int totalNames = dto.CertificationName.Count;
                int totalNew = addCertificates.Count;

                // New certificate name/code are always LAST items in arrays
                for (int i = 0; i < totalNew; i++)
                {
                    int idx = totalNames - totalNew + i;

                    var cert = new ProductCertificate
                    {
                        RegistrationId = entity.Id,
                        ProductId = null,
                        CertificationName = dto.CertificationName[idx],
                        CertificationCode = dto.CertificationCode[idx],
                        Status = ProductCertificateStatus.Pending,
                        UploadedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.ProductCertificates.AddAsync(cert, ct);
                    await _db.SaveChangesAsync(ct);

                    var file = addCertificates[i];

                    var media = new MediaLink
                    {
                        OwnerType = MediaOwnerType.ProductCertificates,
                        OwnerId = cert.Id,
                        ImagePublicId = file.ImagePublicId,
                        ImageUrl = file.ImageUrl,
                        Purpose = MediaPurpose.ProductCertificatePdf,
                        SortOrder = i + 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.MediaLinks.AddAsync(media, ct);
                    await _db.SaveChangesAsync(ct);
                }
            }

            var fresh = await _db.ProductRegistrations.AsNoTracking()
                .FirstAsync(x => x.Id == entity.Id, ct);

            var result = _mapper.Map<ProductRegistrationReponseDTO>(fresh);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { result }, ct);

            return result;
        }


        // ================= CHANGE STATUS =================

        public async Task<bool> ChangeStatusAsync(
    ulong id,
    ProductRegistrationStatus status,
    string? rejectionReason,
    ulong? approvedBy,
    CancellationToken ct = default)
        {
            // 1) Load registration
            var reg = await _db.ProductRegistrations
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (reg == null)
                throw new InvalidOperationException("Đơn đăng ký không tồn tại.");

            if (reg.Status == ProductRegistrationStatus.Approved)
                throw new InvalidOperationException("Đơn đăng ký đã được duyệt trước đó.");

            if (reg.Status == ProductRegistrationStatus.Rejected)
                throw new InvalidOperationException("Đơn đăng ký đã bị từ chối trước đó.");

            reg.UpdatedAt = DateTime.UtcNow;

            // =======================
            // CASE REJECT
            // =======================
            if (status == ProductRegistrationStatus.Rejected)
            {
                reg.Status = ProductRegistrationStatus.Rejected;
                reg.RejectionReason = rejectionReason ?? "";

                // Chỉ select Id để tránh NULL cast lỗi
                var certIds = await _db.ProductCertificates
                    .Where(c => c.RegistrationId == reg.Id)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                foreach (var certId in certIds)
                {
                    // Không dùng FirstAsync nữa
                    var entityCert = new ProductCertificate { Id = certId };
                    _db.ProductCertificates.Attach(entityCert);

                    entityCert.Status = ProductCertificateStatus.Rejected;
                    entityCert.RejectionReason = rejectionReason ?? "";
                    entityCert.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync(ct);
                return true;
            }


            // =======================
            // CASE APPROVE
            // =======================
            if (status != ProductRegistrationStatus.Approved)
                throw new InvalidOperationException("Trạng thái không hợp lệ.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                // update registration
                reg.Status = ProductRegistrationStatus.Approved;
                reg.ApprovedBy = approvedBy;
                reg.ApprovedAt = DateTime.UtcNow;
                reg.RejectionReason = null;

                await _db.SaveChangesAsync(ct);

                // Tạo product
                var product = _mapper.Map<Product>(reg);
                product.Slug = Slugify(reg.ProposedProductName);
                product.IsActive = true;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _db.Products.Add(product);
                await _db.SaveChangesAsync(ct);   // now product.Id is VALID and SAFE

                // Copy images
                var regImages = await _db.MediaLinks
                    .Where(m => m.OwnerType == MediaOwnerType.ProductRegistrations && m.OwnerId == reg.Id)
                    .OrderBy(m => m.SortOrder)
                    .ToListAsync(ct);

                if (regImages.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    var clones = regImages.Select(m => new MediaLink
                    {
                        OwnerType = MediaOwnerType.Products,
                        OwnerId = product.Id,
                        ImagePublicId = m.ImagePublicId,
                        ImageUrl = m.ImageUrl,
                        Purpose = MediaPurpose.ProductImage,
                        SortOrder = m.SortOrder,
                        CreatedAt = now,
                        UpdatedAt = now
                    }).ToList();

                    _db.MediaLinks.AddRange(clones);
                    await _db.SaveChangesAsync(ct);
                }

                // Copy certificates
                var regCerts = await _db.ProductCertificates
                    .Where(c => c.RegistrationId == reg.Id)
                    .Select(c => new
                    {
                        c.Id,
                        c.CertificationCode,
                        c.CertificationName
                    })
                    .ToListAsync(ct);




                foreach (var old in regCerts)
                {
                    var newCert = new ProductCertificate
                    {
                        ProductId = product.Id,
                        RegistrationId = null,
                        CertificationName = old.CertificationName,
                        CertificationCode = old.CertificationCode,
                        Status = ProductCertificateStatus.Verified,
                        VerifiedBy = approvedBy,
                        VerifiedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UploadedAt = DateTime.UtcNow
                    };

                    _db.ProductCertificates.Add(newCert);
                    await _db.SaveChangesAsync(ct);

                    // Clone certificate files
                    var files = await _db.MediaLinks
                        .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates &&
                                    m.OwnerId == old.Id)
                        .OrderBy(m => m.SortOrder)
                        .ToListAsync(ct);

                    if (files.Count > 0)
                    {
                        var now = DateTime.UtcNow;
                        var fclones = files.Select(f => new MediaLink
                        {
                            OwnerType = MediaOwnerType.ProductCertificates,
                            OwnerId = newCert.Id,
                            ImagePublicId = f.ImagePublicId,
                            ImageUrl = f.ImageUrl,
                            Purpose = MediaPurpose.ProductCertificatePdf,
                            SortOrder = f.SortOrder,
                            CreatedAt = now,
                            UpdatedAt = now
                        })
                        .ToList();

                        _db.MediaLinks.AddRange(fclones);
                        await _db.SaveChangesAsync(ct);
                    }
                }

                await tx.CommitAsync(ct);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }



        // ================= DELETE =================

        public Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);


        // ================ HELPERS =================

        private static string Slugify(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Guid.NewGuid().ToString("n");
            var s = text.Trim().ToLowerInvariant();
            s = s.Replace("đ", "d").Normalize(System.Text.NormalizationForm.FormD);

            var filtered = new System.Text.StringBuilder();
            foreach (var c in s)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    filtered.Append(c);
            }

            s = filtered.ToString().Normalize(System.Text.NormalizationForm.FormC);

            var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
            s = new string(chars);

            while (s.Contains("--")) s = s.Replace("--", "-");
            s = s.Trim('-');

            return s == "" ? Guid.NewGuid().ToString("n") : s;
        }


        // Hydrate images & certificates
        private async Task HydrateMediaAsync(
            IReadOnlyList<ProductRegistrationReponseDTO> items,
            CancellationToken ct)
        {
            if (items.Count == 0) return;

            var ids = items.Select(x => x.Id).ToList();
            var map = items.ToDictionary(x => x.Id);

            // Product images
            var imgs = await _db.MediaLinks.AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.ProductRegistrations &&
                            ids.Contains(m.OwnerId))
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            foreach (var g in imgs.GroupBy(m => m.OwnerId))
                if (map.TryGetValue(g.Key, out var dto))
                    dto.ProductImages = g
                        .Select(m => new MediaLinkItemDTO
                        {
                            Id = m.Id,
                            ImagePublicId = m.ImagePublicId,
                            ImageUrl = m.ImageUrl,
                            Purpose = m.Purpose.ToString(),
                            SortOrder = m.SortOrder
                        })
                        .ToList();


            // Certificates (entity + files)
            var certEntities = await _db.ProductCertificates.AsNoTracking()
               .Where(c => c.RegistrationId.HasValue && ids.Contains(c.RegistrationId.Value))
               .Select(c => new
               {
                   c.Id,
                   c.RegistrationId,
                   c.CertificationCode,
                   c.CertificationName
               })
               .ToListAsync(ct);

            foreach (var cert in certEntities)
            {
                var files = await _db.MediaLinks.AsNoTracking()
                    .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates &&
                                m.OwnerId == cert.Id)
                    .OrderBy(m => m.SortOrder)
                    .Select(f => new MediaLinkItemDTO
                    {
                        Id = f.Id,
                        ImagePublicId = f.ImagePublicId,
                        ImageUrl = f.ImageUrl,
                        Purpose = f.Purpose.ToString(),
                        SortOrder = f.SortOrder
                    })
                    .ToListAsync(ct);

                if (cert.RegistrationId.HasValue && map.TryGetValue(cert.RegistrationId.Value, out var dto))
                {
                    dto.CertificationCode.Add(cert.CertificationCode);
                    dto.CertificationName.Add(cert.CertificationName);

                    dto.Certificates.Add(new BLL.DTO.ProductCertificate.ProductCertificateResponseDTO
                    {
                        Id = cert.Id,
                        CertificationName = cert.CertificationName,
                        CertificationCode = cert.CertificationCode,
                        Files = files
                    });
                }
            }
        }


        private static decimal? ParseNullableDecimal(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;

            return null;
        }



        private static Dictionary<string, decimal>? ToDecimalDict(object? input)
        {
            if (input is null) return null;

            if (input is Dictionary<string, decimal> dict) return dict;

            var t = input.GetType();
            decimal? Read(string name)
            {
                var p = t.GetProperty(name) ??
                        t.GetProperty(char.ToUpper(name[0]) + name.Substring(1));
                if (p == null) return null;

                var v = p.GetValue(input);
                if (v == null) return null;

                if (v is decimal d) return d;
                if (v is double d2) return (decimal)d2;
                if (v is float f) return (decimal)f;
                if (v is int i) return i;
                if (v is long l) return l;
                if (v is string s && decimal.TryParse(s, out var parsed)) return parsed;

                return null;
            }

            var result = new Dictionary<string, decimal>();
            var w = Read("width");
            var h = Read("height");
            var l2 = Read("length");

            if (w.HasValue) result["width"] = w.Value;
            if (h.HasValue) result["height"] = h.Value;
            if (l2.HasValue) result["length"] = l2.Value;

            return result.Count == 0 ? null : result;
        }

        private static List<MediaLink>? ToMediaLinks(
            IEnumerable<MediaLinkItemDTO>? src,
            MediaOwnerType ownerType)
        {
            if (src == null) return null;

            var result = new List<MediaLink>();
            int sort = 0;
            var now = DateTime.UtcNow;

            foreach (var i in src)
            {
                result.Add(new MediaLink
                {
                    OwnerType = ownerType,
                    OwnerId = 0,
                    ImagePublicId = i.ImagePublicId,
                    ImageUrl = i.ImageUrl,
                    Purpose = ownerType == MediaOwnerType.ProductRegistrations
                        ? MediaPurpose.ProductImage
                        : MediaPurpose.ProductCertificatePdf,
                    SortOrder = ++sort,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            return result;
        }

        private static PagedResponse<T> ToPaged<T>(
            List<T> items, int totalRecords, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling(totalRecords * 1.0 / pageSize);
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
    }
}
