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
using BLL.Interfaces.Infrastructure;

namespace BLL.Services
{
    public class ProductRegistrationService : IProductRegistrationService
    {
        private readonly IProductRegistrationRepository _repo;
        private readonly IMapper _mapper;
        private readonly VerdantTechDbContext _db;
        private readonly IEmailSender _emailSender;


        public ProductRegistrationService(
            IProductRegistrationRepository repo,
            IMapper mapper,
            VerdantTechDbContext db,
            IEmailSender emailSender
            )
        {
            _repo = repo;
            _mapper = mapper;
            _db = db;
            _emailSender = emailSender;
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

            var productImages = ToMediaLinks(addImages, MediaOwnerType.ProductRegistrations);

            // Validate certificates trước khi tạo ProductRegistration
            if (dto.CertificationName != null && dto.CertificationCode != null)
            {
                if (addCertificates.Count != dto.CertificationName.Count)
                    throw new InvalidOperationException("Số lượng file chứng chỉ không khớp số lượng tên chứng chỉ.");

                if (dto.CertificationName.Count != dto.CertificationCode.Count)
                    throw new InvalidOperationException("Tên chứng chỉ và mã chứng chỉ không khớp số lượng.");
            }

            // Tạo ProductRegistration (có transaction bên trong Repository)
            entity = await _repo.CreateAsync(entity, productImages, null, ct);

            // Tạo certificates sau khi ProductRegistration đã được tạo thành công
            // Nếu có lỗi ở đây, sẽ cleanup ProductRegistration đã tạo
            if (dto.CertificationName != null && dto.CertificationCode != null)
            {
                try
                {
                    for (int i = 0; i < dto.CertificationName.Count; i++)
                    {
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
                catch (Exception)
                {
                    // Nếu có lỗi khi tạo certificates, cleanup ProductRegistration đã tạo
                    try
                    {
                        await _repo.DeleteAsync(entity.Id, ct);
                    }
                    catch (Exception cleanupEx)
                    {
                        // Log cleanup error nhưng vẫn throw original exception
                        // để Controller có thể cleanup files trên Cloudinary
                        System.Diagnostics.Debug.WriteLine($"Failed to cleanup ProductRegistration {entity.Id}: {cleanupEx.Message}");
                    }
                    
                    // Throw exception để Controller có thể cleanup files trên Cloudinary
                    throw;
                }
            }

            var fresh = await _db.ProductRegistrations.AsNoTracking()
                .FirstAsync(x => x.Id == entity.Id, ct);

            var result = _mapper.Map<ProductRegistrationReponseDTO>(fresh);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { result }, ct);

            // ==== SEND EMAIL====
            var vendor = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.VendorId, ct);
            if (vendor != null)
            {
                await _emailSender.SendProductRegistrationSubmittedEmailAsync(
                    vendor.Email!,
                    vendor.FullName ?? vendor.Email!,
                    dto.ProposedProductName,
                    ct
                );
            }

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

            if (addCertificates.Count > 0 &&
                dto.CertificationName != null &&
                dto.CertificationCode != null)
            {
                int totalNames = dto.CertificationName.Count;
                int totalNew = addCertificates.Count;

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
            var reg = await _db.ProductRegistrations
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (reg == null)
                throw new InvalidOperationException("Đơn đăng ký không tồn tại.");

            if (reg.Status == ProductRegistrationStatus.Approved)
                throw new InvalidOperationException("Đơn đăng ký đã được duyệt trước đó.");

            if (reg.Status == ProductRegistrationStatus.Rejected)
                throw new InvalidOperationException("Đơn đăng ký đã bị từ chối trước đó.");

            reg.UpdatedAt = DateTime.UtcNow;

            // CASE REJECT
            if (status == ProductRegistrationStatus.Rejected)
            {
                reg.Status = ProductRegistrationStatus.Rejected;
                reg.RejectionReason = rejectionReason ?? "";

                var certIds = await _db.ProductCertificates
                    .Where(c => c.RegistrationId == reg.Id)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                foreach (var certId in certIds)
                {
                    var entityCert = new ProductCertificate { Id = certId };
                    _db.ProductCertificates.Attach(entityCert);

                    entityCert.Status = ProductCertificateStatus.Rejected;
                    entityCert.RejectionReason = rejectionReason ?? "";
                    entityCert.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync(ct);


                // ==== SEND EMAIL: PRODUCT REJECTED ====
                var vendor = await _db.Users.FirstOrDefaultAsync(u => u.Id == reg.VendorId, ct);
                if (vendor != null)
                {
                    await _emailSender.SendProductRegistrationRejectedEmailAsync(
                        vendor.Email!,
                        vendor.FullName ?? vendor.Email!,
                        reg.ProposedProductName,
                        rejectionReason ?? "",
                        ct
                    );
                }


                return true;
            }


            // CASE APPROVE
            if (status != ProductRegistrationStatus.Approved)
                throw new InvalidOperationException("Trạng thái không hợp lệ.");

            using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                reg.Status = ProductRegistrationStatus.Approved;
                reg.ApprovedBy = approvedBy;
                reg.ApprovedAt = DateTime.UtcNow;
                reg.RejectionReason = null;

                await _db.SaveChangesAsync(ct);

                var product = _mapper.Map<Product>(reg);
                product.Slug = Slugify(reg.ProposedProductName);
                product.IsActive = true;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _db.Products.Add(product);
                await _db.SaveChangesAsync(ct);  

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

                // ==== SEND EMAIL: PRODUCT APPROVED ====
                var vendor = await _db.Users.FirstOrDefaultAsync(u => u.Id == reg.VendorId, ct);
                if (vendor != null)
                {
                    await _emailSender.SendProductRegistrationApprovedEmailAsync(
                        vendor.Email!,
                        vendor.FullName ?? vendor.Email!,
                        reg.ProposedProductName,
                        ct
                    );
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


        private async Task HydrateMediaAsync(
            IReadOnlyList<ProductRegistrationReponseDTO> items,
            CancellationToken ct)
        {
            if (items.Count == 0) return;

            var ids = items.Select(x => x.Id).ToList();
            var map = items.ToDictionary(x => x.Id);

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
