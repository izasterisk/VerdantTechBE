using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Swashbuckle.AspNetCore.Annotations;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
using Infrastructure.Cloudinary;
namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductRegistrationsController : ControllerBase
    {
        private readonly IProductRegistrationService _service;
        private readonly ICloudinaryService _cloud;
        private readonly ILogger<ProductRegistrationsController> _logger;

        public ProductRegistrationsController(
            IProductRegistrationService service,
            ICloudinaryService cloud,
            ILogger<ProductRegistrationsController> logger)
        {
            _service = service;
            _cloud = cloud;
            _logger = logger;
        }
        private void LogDict(string label, Dictionary<string, object>? dict)
        {
            try
            {
                var json = dict is null ? "null" : System.Text.Json.JsonSerializer.Serialize(dict);
                _logger.LogInformation("{Label}: {Json}", label, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Serialize specs failed in controller");
            }
        }

        // ========= READ =========

        [HttpGet]
        [EndpointSummary("Danh sách đăng ký sản phẩm (có phân trang)")]
        [EndpointDescription("Trả về danh sách ProductRegistration kèm ảnh (MediaLinks) và manual URLs nếu có.")]
        public async Task<ActionResult<PagedResponse<ProductRegistrationReponseDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await _service.GetAllAsync(page, pageSize, ct);
            return Ok(res);
        }

        [HttpGet("{id:long}")]
        [EndpointSummary("Chi tiết đăng ký sản phẩm theo Id")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> GetById(
            ulong id,
            CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound("Đơn đăng ký không tồn tại.") : Ok(item);
        }

        [HttpGet("vendor/{vendorId:long}")]
        [EndpointSummary("Danh sách đăng ký theo Vendor (có phân trang)")]
        public async Task<ActionResult<PagedResponse<ProductRegistrationReponseDTO>>> GetByVendor(
            ulong vendorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await _service.GetByVendorAsync(vendorId, page, pageSize, ct);
            return Ok(res);
        }

        // ========= CREATE =========

        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo đăng ký sản phẩm (multipart/form-data)")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Create(
            [FromForm] CreateForm req,
            CancellationToken ct = default)
        {
            FillSpecsFromForm(req.Data, Request.Form);
            // Manual
            string? manualUrl = null, manualPublicUrl = null;
            if (req.ManualFile is not null)
            {
                var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                if (up is not null) { manualUrl = up.Url; manualPublicUrl = up.PublicUrl; }
            }

            // Images
            var imagesDto = new List<MediaLinkItemDTO>();
            if (req.Images is { Count: > 0 })
            {
                var ups = await _cloud.UploadManyAsync(req.Images, "product-registrations/images", ct);
                imagesDto = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();
            }

            // Certificates
            var certDtos = new List<MediaLinkItemDTO>();
            if (req.Certificate is { Count: > 0 })
            {
                // (tuỳ chọn) chặn file không phải PDF
                var pdfs = req.Certificate.Where(f =>
                    string.Equals(f.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    f.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                var ups = await _cloud.UploadManyAsync(pdfs, "product-registrations/certificates", ct);
                certDtos = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.PublicUrl,     // ✅ dùng PublicUrl
                    Purpose = "CertificatePdf",// ✅ gắn đúng purpose
                    SortOrder = i + 1
                }).ToList();
            }

            var created = await _service.CreateAsync(
                req.Data,
                manualUrl, manualPublicUrl,
                imagesDto,
                certDtos,    // ✅ truyền vào
                ct
            );
            return Ok(created);
        } 

            // ========= UPDATE =========

        [HttpPut("{id:long}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật đăng ký sản phẩm (multipart/form-data)")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Update(
            ulong id,
            [FromForm] UpdateForm req,
            CancellationToken ct = default)
        {
            FillSpecsFromForm(req.Data, Request.Form);
            // Manual
            string? manualUrl = null, manualPublicUrl = null;
            if (req.ManualFile is not null)
            {
                var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                if (up is not null) { manualUrl = up.Url; manualPublicUrl = up.PublicUrl; }
            }

            // Thêm ảnh
            var addImages = new List<MediaLinkItemDTO>();
            if (req.Images is { Count: > 0 })
            {
                var ups = await _cloud.UploadManyAsync(req.Images, "product-registrations/images", ct);
                addImages = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();
            }

            // Thêm chứng chỉ
            var addCertificates = new List<MediaLinkItemDTO>();
            if (req.Certificate is { Count: > 0 })
            {
                var pdfs = req.Certificate.Where(f =>
                    string.Equals(f.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    f.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                var ups = await _cloud.UploadManyAsync(pdfs, "product-registrations/certificates", ct);
                addCertificates = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.PublicUrl,     // ✅ dùng PublicUrl
                    Purpose = "CertificatePdf",// ✅ gắn đúng purpose
                    SortOrder = i + 1
                }).ToList();
            }

            var removedImages = req.RemoveImagePublicIds ?? new List<string>();
            var removedCerts = req.RemoveCertificatePublicIds ?? new List<string>();

            var updated = await _service.UpdateAsync(
                req.Data,
                manualUrl, manualPublicUrl,
                addImages,
                addCertificates,           // ✅ truyền vào
                removedImages,
                removedCerts,
                ct
            );
            return Ok(updated);
        }

        // ========= CHANGE STATUS / DELETE =========

        [HttpPatch("{id:long}/status")]
        [EndpointSummary("Duyệt / Từ chối đơn đăng ký")]
        public async Task<IActionResult> ChangeStatus(
            ulong id,
            [FromBody] ProductRegistrationChangeStatusDTO dto,
            CancellationToken ct = default)
        {
            var ok = await _service.ChangeStatusAsync(id, dto.Status, dto.RejectionReason, dto.ApprovedBy, ct);
            return ok ? NoContent() : NotFound("Đơn đăng ký không tồn tại.");
        }

        [HttpDelete("{id:long}")]
        [EndpointSummary("Xoá đơn đăng ký sản phẩm")]
        public async Task<IActionResult> Delete(ulong id, CancellationToken ct = default)
        {
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound("Đơn đăng ký không tồn tại.");
        }

        // ========= Request models =========

        public sealed class CreateForm
        {
            [FromForm] public ProductRegistrationCreateDTO Data { get; set; } = null!;
            [FromForm] public IFormFile? ManualFile { get; set; }
            [FromForm] public List<IFormFile>? Images { get; set; }
            [FromForm] public List<IFormFile>? Certificate { get; set; }
        }

        public sealed class UpdateForm
        {
            [FromForm] public ProductRegistrationUpdateDTO Data { get; set; } = null!;
            [FromForm] public IFormFile? ManualFile { get; set; }
            [FromForm] public List<IFormFile>? Images { get; set; }
            [FromForm] public List<IFormFile>? Certificate { get; set; }
            [FromForm] public List<string>? RemoveImagePublicIds { get; set; }
            [FromForm] public List<string>? RemoveCertificatePublicIds { get; set; }
        }

        // ========= Helpers (bind Specifications từ multipart/form-data) =========

        private static readonly HashSet<string> KnownFormKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        // file fields
        "ManualFile", "Images", "Certificate",

        // all Data.* fields except Specifications
        "Data.VendorId","Data.CategoryId","Data.ProposedProductCode","Data.ProposedProductName",
        "Data.Description","Data.UnitPrice","Data.EnergyEfficiencyRating","Data.WarrantyMonths",
        "Data.WeightKg","Data.DimensionsCm.Width","Data.DimensionsCm.Height","Data.DimensionsCm.Length",
        "Data.Id", // for update
        // nếu bạn còn field nào khác trong DTO thì thêm vào đây
        // KHÔNG thêm "Data.Specifications" vì ta sẽ tự xử lý riêng
    };

        // ===== helper: merge specs vào DTO =====
        private static void FillSpecsFromForm(object dto, IFormCollection form)
        {
            // 1) Nếu client gửi đúng "Data.Specifications" là JSON string → parse dùng luôn
            if (form.TryGetValue("Data.Specifications", out var jsonVals) &&
                !string.IsNullOrWhiteSpace(jsonVals.ToString()))
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonVals.ToString())
                               ?? new Dictionary<string, object>();
                    dto.GetType().GetProperty("Specifications")?.SetValue(dto, dict);
                    return;
                }
                catch { /* rơi xuống bước 2 */ }
            }

            // 2) Swagger thường "flatten" các cặp key (N, P2O5, ...) thành field rời ở gốc form.
            //    Ta gom TẤT CẢ key KHÔNG thuộc KnownFormKeys & KHÔNG bắt đầu bằng "Data."
            var specs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in form)
            {
                var key = kv.Key;
                if (key.StartsWith("Data.", StringComparison.OrdinalIgnoreCase)) continue;
                if (KnownFormKeys.Contains(key)) continue;

                var raw = kv.ToString();
                // cố gắng parse số → nếu fail thì để string
                if (decimal.TryParse(raw, out var dec)) specs[key] = dec;
                else if (int.TryParse(raw, out var i)) specs[key] = i;
                else specs[key] = raw;
            }

            if (specs.Count > 0)
            {
                dto.GetType().GetProperty("Specifications")?.SetValue(dto, specs);
            }
        }

        private static void BindSpecsFromFormIfAny(HttpRequest request, out Dictionary<string, object>? parsed)
        {
            parsed = null;
            if (!request.HasFormContentType) return;

            var form = request.Form;

            // 1) Ưu tiên đọc JSON nguyên khối
            var jsonKeys = new[]
            {
                "Data.Specifications",
                "Specifications",
                "data.specifications",
                "specifications"
            };

            foreach (var key in jsonKeys)
            {
                if (form.TryGetValue(key, out var raw) && !StringValues.IsNullOrEmpty(raw))
                {
                    var s = raw.ToString();
                    var t = s.Trim();
                    if (!string.IsNullOrWhiteSpace(t) &&
                        ((t.StartsWith("{") && t.EndsWith("}")) || (t.StartsWith("[") && t.EndsWith("]"))))
                    {
                        try
                        {
                            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(t);
                            if (dict != null && dict.Count > 0) { parsed = dict; return; }
                        }
                        catch { /* ignore */ }
                    }
                }
            }

            // 2) Fallback: bracket-form Data.Specifications[key]=value
            var prefixes = new[] { "Data.Specifications[", "Specifications[" };
            Dictionary<string, object>? acc = null;

            foreach (var fk in form.Keys)
            {
                foreach (var p in prefixes)
                {
                    if (fk.StartsWith(p, StringComparison.OrdinalIgnoreCase) && fk.EndsWith("]"))
                    {
                        var propName = fk.Substring(p.Length, fk.Length - p.Length - 1);
                        var val = form[fk].ToString();

                        acc ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        acc[propName] = CoerceToBestType(val);
                    }
                }
            }

            if (acc != null && acc.Count > 0) parsed = acc;
        }

        private static object CoerceToBestType(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            if (bool.TryParse(s, out var b)) return b;
            if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return l;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)) return dec;

            var t = s.Trim();
            if ((t.StartsWith("{") && t.EndsWith("}")) || (t.StartsWith("[") && t.EndsWith("]")))
            {
                try { return JsonSerializer.Deserialize<object>(t) ?? s; }
                catch { /* keep as string */ }
            }
            return s;
        }
    }
}
