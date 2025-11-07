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
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        // ================ CREATE (ảnh + manual) ================

        public async Task<ProductRegistrationReponseDTO> CreateAsync( ProductRegistrationCreateDTO dto, string? manualUrl, string? manualPublicUrl, List<MediaLinkItemDTO> addImages, List<MediaLinkItemDTO> addCertificates, CancellationToken ct = default)
        {
            // validate FK
            if (!await _db.Users.AnyAsync(x => x.Id == dto.VendorId, ct))
                throw new InvalidOperationException("Vendor không tồn tại.");
            if (!await _db.ProductCategories.AnyAsync(x => x.Id == dto.CategoryId, ct))
                throw new InvalidOperationException("Category không tồn tại.");
            var rating = ParseNullableInt(dto.EnergyEfficiencyRating);
            if (rating is < 0 or > 5)
                throw new InvalidOperationException("EnergyEfficiencyRating phải từ 0 đến 5.");

            var dims = ToDecimalDict(dto.DimensionsCm);
            if (dto.DimensionsCm != null && dims is null)
                throw new InvalidOperationException("Kích thước không hợp lệ (width/height/length).");



            var entity = _mapper.Map<ProductRegistration>(dto);

            // đồng bộ kiểu
            entity.Specifications = dto.Specifications ?? new Dictionary<string, object>();
            //entity.Specifications = ParseSpecs(dto);
            entity.EnergyEfficiencyRating = ParseNullableInt(dto.EnergyEfficiencyRating);
            entity.DimensionsCm = ToDecimalDict(dto.DimensionsCm) ?? new Dictionary<string, decimal>();

            entity.Status = ProductRegistrationStatus.Pending;
            entity.ManualUrls = manualUrl;
            entity.PublicUrl = manualPublicUrl;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;


            // map ảnh sang MediaLink để repo insert cùng lúc

            var productImages = ToMediaLinks(addImages, MediaOwnerType.ProductRegistrations, 0);
            var certificateImages = ToMediaLinks(addCertificates, MediaOwnerType.ProductCertificates, 0);


            entity = await _repo.CreateAsync( entity, productImages, certificateImages, ct);
            var fresh = await _db.ProductRegistrations
                .AsNoTracking()
                .FirstAsync(x => x.Id == entity.Id, ct);

            var result = _mapper.Map<ProductRegistrationReponseDTO>(fresh);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { result }, ct);
            result.EnergyEfficiencyRating = entity.EnergyEfficiencyRating?.ToString();


            return result;
        }

        // ================ UPDATE (thêm/bớt ảnh + manual) ================

        public async Task<ProductRegistrationReponseDTO> UpdateAsync( ProductRegistrationUpdateDTO dto, string? manualUrl, string? manualPublicUrl, List<MediaLinkItemDTO> addImages, List<MediaLinkItemDTO> addCertificates, List<string> removedImages, List<string> removedCertificates, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(dto.Id, ct)
                         ?? throw new KeyNotFoundException("Đơn đăng ký không tồn tại.");

            var newRating = ParseNullableInt(dto.EnergyEfficiencyRating);
            if (newRating is < 0 or > 5)
                throw new InvalidOperationException("EnergyEfficiencyRating phải từ 0 đến 5.");

            var newDims = ToDecimalDict(dto.DimensionsCm);
            if (dto.DimensionsCm != null && newDims is null)
                throw new InvalidOperationException("Kích thước không hợp lệ (width/height/length).");


            // update các field cơ bản
            entity.VendorId = dto.VendorId;
            entity.CategoryId = dto.CategoryId;
            entity.ProposedProductCode = dto.ProposedProductCode;
            entity.ProposedProductName = dto.ProposedProductName;
            entity.Description = dto.Description;
            entity.UnitPrice = dto.UnitPrice;
            entity.EnergyEfficiencyRating = ParseNullableInt(dto.EnergyEfficiencyRating);

            entity.Specifications = dto.Specifications ?? entity.Specifications ?? new Dictionary<string, object>();

            //var parsedSpecs = ParseSpecs(dto);
            //if (dto.Specifications != null || !string.IsNullOrWhiteSpace(dto.SpecificationsJson))
            //    entity.Specifications = parsedSpecs;

            //if (dto.Specifications != null)
            //{
            //    entity.Specifications = dto.Specifications;
            //}

            entity.DimensionsCm = ToDecimalDict(dto.DimensionsCm) ?? entity.DimensionsCm ?? new Dictionary<string, decimal>();

            if (!string.IsNullOrWhiteSpace(manualUrl)) entity.ManualUrls = manualUrl;
            if (!string.IsNullOrWhiteSpace(manualPublicUrl)) entity.PublicUrl = manualPublicUrl;
            entity.UpdatedAt = DateTime.UtcNow;

            var addProductImages = ToMediaLinks(addImages, MediaOwnerType.ProductRegistrations, 0);
            var addCertificateImages = ToMediaLinks(addCertificates, MediaOwnerType.ProductCertificates, 0);
            //var removeImagePublicIds = removed ?? new List<string>();

            entity = await _repo.UpdateAsync( entity, addProductImages, addCertificateImages, removedImages ?? new List<string>(), removedCertificates ?? new List<string>(), ct);

            

            var fresh = await _db.ProductRegistrations
                .AsNoTracking()
                .FirstAsync(x => x.Id == entity.Id, ct);

            var result = _mapper.Map<ProductRegistrationReponseDTO>(fresh);
            await HydrateMediaAsync(new List<ProductRegistrationReponseDTO> { result }, ct);
            result.EnergyEfficiencyRating = entity.EnergyEfficiencyRating?.ToString();
            return result;
        }

        // ================ STATUS / DELETE ================

        //public async Task<bool> ChangeStatusAsync(ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default)
        //{

        //    var ok = await _repo.ChangeStatusAsync(id, status, rejectionReason, approvedBy, status == ProductRegistrationStatus.Approved ? DateTime.UtcNow : (DateTime?)null, ct);
        //    if (!ok) throw new KeyNotFoundException("Đơn đăng ký không tồn tại.");

        //    var approvedAt = status == ProductRegistrationStatus.Approved
        //        ? DateTime.UtcNow
        //        : (DateTime?)null;


        //    return await _repo.ChangeStatusAsync(id, status, rejectionReason, approvedBy, approvedAt, ct);
        //}

        public async Task<bool> ChangeStatusAsync( ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default)
        {
            // Lấy đơn
            var reg = await _db.ProductRegistrations
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (reg == null) return false;

            // Nếu không đổi gì thì thôi
            if (reg.Status == status &&
                reg.RejectionReason == rejectionReason &&
                reg.ApprovedBy == approvedBy)
            {
                return true;
            }

            // Cập nhật trạng thái cơ bản
            reg.Status = status;
            reg.RejectionReason = status == ProductRegistrationStatus.Rejected ? (rejectionReason ?? string.Empty) : null;
            reg.ApprovedBy = status == ProductRegistrationStatus.Approved ? approvedBy : null;
            reg.ApprovedAt = status == ProductRegistrationStatus.Approved ? DateTime.UtcNow : (DateTime?)null;
            reg.UpdatedAt = DateTime.UtcNow;

            // Nếu không phải Approved => chỉ save thay đổi trạng thái
            if (status != ProductRegistrationStatus.Approved)
            {
                await _db.SaveChangesAsync(ct);
                return true;
            }

            // -----------------------------
            // Approved: tạo Product + copy media
            // -----------------------------
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1) Map qua Product
                //    Lưu ý: bạn đã cấu hình AutoMapper ProductRegistration -> Product rồi.
                var product = _mapper.Map<Product>(reg);

                // Bổ sung các field nên set tại service
                product.Slug = Slugify(reg.ProposedProductName);
                product.IsActive = true;                    // tuỳ mô hình
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                // Nếu Product có các trường có default hoặc do DB sinh thì bỏ qua
                _db.Products.Add(product);
                await _db.SaveChangesAsync(ct); // cần ID của product để copy media

                // 2) Copy media từ registration → product
                //    2.1) Ảnh sản phẩm
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
                        Purpose = m.Purpose,
                        SortOrder = m.SortOrder,
                        CreatedAt = now,
                        UpdatedAt = now
                    }).ToList();

                    _db.MediaLinks.AddRange(clones);
                    await _db.SaveChangesAsync(ct);
                }

                //    2.2) (Tuỳ chọn) Copy certificates để Product cũng có chứng chỉ
                var regCerts = await _db.MediaLinks
                    .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && m.OwnerId == reg.Id)
                    .OrderBy(m => m.SortOrder)
                    .ToListAsync(ct);

                if (regCerts.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    var certClones = regCerts.Select(m => new MediaLink
                    {
                        OwnerType = MediaOwnerType.ProductCertificates, // giữ nguyên type
                        OwnerId = product.Id,                         // nhưng trỏ sang product
                        ImagePublicId = m.ImagePublicId,
                        ImageUrl = m.ImageUrl,
                        Purpose = m.Purpose,
                        SortOrder = m.SortOrder,
                        CreatedAt = now,
                        UpdatedAt = now
                    }).ToList();

                    _db.MediaLinks.AddRange(certClones);
                    await _db.SaveChangesAsync(ct);
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



        public Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);




        // ================= Helpers =================

        private static string Slugify(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Guid.NewGuid().ToString("n");
            var s = text.Trim().ToLowerInvariant();

            // bỏ dấu tiếng Việt & kí tự đặc biệt cơ bản
            s = s
                .Replace("đ", "d")
                .Normalize(System.Text.NormalizationForm.FormD);
            var filtered = new System.Text.StringBuilder();
            foreach (var c in s)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    filtered.Append(c);
            }
            s = filtered.ToString().Normalize(System.Text.NormalizationForm.FormC);

            // thay thế khoảng trắng/ký tự lạ bằng '-'
            var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
            s = new string(chars);

            // gộp nhiều dấu '-' liên tiếp
            while (s.Contains("--")) s = s.Replace("--", "-");
            s = s.Trim('-');

            return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("n") : s;
        }



        //private async Task HydrateMediaAsync( IReadOnlyList<ProductRegistrationReponseDTO> items, CancellationToken ct)
        //{
        //    if (items.Count == 0) return;
        //    var ids = items.Select(x => x.Id).ToList();
        //    var map = items.ToDictionary(x => x.Id);

        //    var regMedias = await _db.MediaLinks.AsNoTracking()
        //        .Where(m => m.OwnerType == MediaOwnerType.ProductRegistrations && ids.Contains(m.OwnerId))
        //        .OrderBy(m => m.OwnerId).ThenBy(m => m.SortOrder)
        //        .ToListAsync(ct);

        //    var byId = items.ToDictionary(x => x.Id);

        //    foreach (var g in regMedias.GroupBy(x => x.OwnerId))
        //    {
        //        if (!byId.TryGetValue(g.Key, out var dto)) continue;
        //        dto.ProductImages = g.Select(m => new MediaLinkItemDTO
        //        {
        //            Id = m.Id,
        //            ImagePublicId = m.ImagePublicId,
        //            ImageUrl = m.ImageUrl,
        //            Purpose = m.Purpose.ToString().ToLowerInvariant(),
        //            SortOrder = m.SortOrder
        //        }).ToList();
        //    }
        //}


        private async Task HydrateMediaAsync( IReadOnlyList<ProductRegistrationReponseDTO> items, CancellationToken ct)
        {
            if (items.Count == 0) return;
            var ids = items.Select(x => x.Id).ToList();
            var map = items.ToDictionary(x => x.Id);

            // images
            var imgs = await _db.MediaLinks.AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.ProductRegistrations && ids.Contains(m.OwnerId))
                .OrderBy(m => m.OwnerId).ThenBy(m => m.SortOrder)
                .ToListAsync(ct);

            foreach (var g in imgs.GroupBy(m => m.OwnerId))
                if (map.TryGetValue(g.Key, out var dto))
                    dto.ProductImages = g.Select(m => new MediaLinkItemDTO
                    {
                        Id = m.Id,
                        ImagePublicId = m.ImagePublicId,
                        ImageUrl = m.ImageUrl,
                        Purpose = m.Purpose.ToString().ToLowerInvariant(),
                        SortOrder = m.SortOrder
                    }).ToList();

            // certificates
            var certs = await _db.MediaLinks.AsNoTracking()
                .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates && ids.Contains(m.OwnerId))
                .OrderBy(m => m.OwnerId).ThenBy(m => m.SortOrder)
                .ToListAsync(ct);

            foreach (var g in certs.GroupBy(m => m.OwnerId))
                if (map.TryGetValue(g.Key, out var dto))
                    dto.CertificateFiles = g.Select(m => new MediaLinkItemDTO
                    {
                        Id = m.Id,
                        ImagePublicId = m.ImagePublicId,
                        ImageUrl = m.ImageUrl,
                        Purpose = m.Purpose.ToString().ToLowerInvariant(),
                        SortOrder = m.SortOrder
                    }).ToList();
        }


        private static PagedResponse<T> ToPaged<T>(
            List<T> items, int totalRecords, int page, int pageSize)
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

        // ------- type helpers -------

        private static int? ParseNullableInt(string? s)
            => int.TryParse(s, out var v) ? v : null;

        // input có thể là Dictionary<string, decimal> hoặc một object có Width/Height/Length (DTO)
        private static Dictionary<string, decimal>? ToDecimalDict(object? input)
        {
            if (input is null) return null;

            if (input is Dictionary<string, decimal> dict) return dict;

            var t = input.GetType();

            decimal? ReadAsDecimal(string propName)
            {
                var p = t.GetProperty(propName) ??
                        t.GetProperty(char.ToUpperInvariant(propName[0]) + propName.Substring(1));
                if (p == null) return null;

                var v = p.GetValue(input);
                if (v is null) return null;

                if (v is decimal d1) return d1;
                if (v is double d2) return (decimal)d2;
                if (v is float f) return (decimal)f;
                if (v is int i) return i;
                if (v is long l) return l;
                if (v is string s && decimal.TryParse(s, out var parsed)) return parsed;

                return null;
            }

            var res = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var w = ReadAsDecimal("width");
            var h = ReadAsDecimal("height");
            var l = ReadAsDecimal("length");

            if (w.HasValue) res["width"] = w.Value;
            if (h.HasValue) res["height"] = h.Value;
            if (l.HasValue) res["length"] = l.Value;

            return res.Count == 0 ? null : res;
        }

        //private static Dictionary<string, object> ParseSpecs(ProductRegistrationCreateDTO dto)
        //{
        //    if (dto.Specifications != null) return dto.Specifications;
        //    if (!string.IsNullOrWhiteSpace(dto.SpecificationsJson))
        //    {
        //        try
        //        {
        //            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.SpecificationsJson);
        //            return dict ?? new Dictionary<string, object>();
        //        }
        //        catch
        //        {
        //            return new Dictionary<string, object>();
        //        }
        //    }
        //    return new Dictionary<string, object>();
        //}

        //private static Dictionary<string, object> ParseSpecs(ProductRegistrationUpdateDTO dto)
        //{
        //    if (dto.Specifications != null) return dto.Specifications;
        //    if (!string.IsNullOrWhiteSpace(dto.SpecificationsJson))
        //    {
        //        try
        //        {
        //            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(dto.SpecificationsJson);
        //            return dict ?? new Dictionary<string, object>();
        //        }
        //        catch
        //        {
        //            return new Dictionary<string, object>();
        //        }
        //    }
        //    return new Dictionary<string, object>();
        //}
        //private static Dictionary<string, object> ParseSpecs(ProductRegistrationCreateDTO dto)
        //{
        //    // Ưu tiên dict đã bind sẵn
        //    if (dto.Specifications is { Count: > 0 }) return dto.Specifications;

        //    // Sau đó mới tới chuỗi JSON
        //    if (!string.IsNullOrWhiteSpace(dto.SpecificationsJson))
        //    {
        //        var raw = dto.SpecificationsJson.Trim();
        //        if (!raw.Equals("string", StringComparison.OrdinalIgnoreCase)) // chặn literal "string"
        //        {
        //            try
        //            {
        //                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
        //                if (dict != null) return dict;
        //            }
        //            catch { /* ignore */ }
        //        }
        //    }
        //    return new Dictionary<string, object>();
        //}

        //private static Dictionary<string, object> ParseSpecs(ProductRegistrationUpdateDTO dto)
        //{
        //    if (dto.Specifications is { Count: > 0 }) return dto.Specifications;

        //    if (!string.IsNullOrWhiteSpace(dto.SpecificationsJson))
        //    {
        //        var raw = dto.SpecificationsJson.Trim();
        //        if (!raw.Equals("string", StringComparison.OrdinalIgnoreCase))
        //        {
        //            try
        //            {
        //                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(raw);
        //                if (dict != null) return dict;
        //            }
        //            catch { /* ignore */ }
        //        }
        //    }
        //    return new Dictionary<string, object>();
        //}



        private static MediaPurpose ParsePurpose(string? purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose)) return MediaPurpose.None;
            return Enum.TryParse<MediaPurpose>(purpose, true, out var p) ? p : MediaPurpose.None;
        }

        private static List<MediaLink>? ToMediaLinks(
            IEnumerable<MediaLinkItemDTO>? src,
            MediaOwnerType ownerType,
            int startSort)
        {
            if (src == null) return null;

            var now = DateTime.UtcNow;
            var sort = startSort;
            var list = new List<MediaLink>();

            foreach (var i in src)
            {
                list.Add(new MediaLink
                {
                    OwnerType = ownerType,
                    OwnerId = 0, // repo sẽ set OwnerId sau khi có Id
                    ImagePublicId = i.ImagePublicId,
                    ImageUrl = i.ImageUrl,
                    Purpose = MediaPurpose.CertificatePdf,
                    SortOrder = ++sort,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            return list;
        }
    }
}
