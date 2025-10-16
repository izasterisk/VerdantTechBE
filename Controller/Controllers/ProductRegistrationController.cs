using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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

        public ProductRegistrationsController(
            IProductRegistrationService service,
            ICloudinaryService cloud)
        {
            _service = service;
            _cloud = cloud;
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
        [EndpointDescription("Bao gồm đầy đủ metadata, danh sách ảnh (MediaLinks) và manual URLs.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> GetById(
            ulong id,
            CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound("Đơn đăng ký không tồn tại.") : Ok(item);
        }

        [HttpGet("vendor/{vendorId:long}")]
        [EndpointSummary("Danh sách đăng ký theo Vendor (có phân trang)")]
        [EndpointDescription("Lọc theo VendorId, trả về danh sách cùng ảnh và manual URLs.")]
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
        [EndpointDescription("Form-data: Data = ProductRegistrationCreateDTO; ManualFile = PDF/tài liệu; Images[] = ảnh sản phẩm. Server sẽ upload Cloudinary và lưu MediaLinks.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Create(
            [FromForm] CreateForm req,
            CancellationToken ct = default)
        {
            // Upload manual (nếu có)
            string? manualUrl = null;
            string? manualPublicUrl = null;
            if (req.ManualFile is not null)
            {
                var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                if (up is not null)
                {
                    manualUrl = up.Url;
                    manualPublicUrl = up.PublicUrl;
                }
            }

            // Upload ảnh (nếu có)
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
                var ups = await _cloud.UploadManyAsync(req.Certificate, "product-registrations/certificates", ct);
                certDtos = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();
            }

            var created = await _service.CreateAsync(req.Data, manualUrl, manualPublicUrl, imagesDto, certDtos, ct);
            return Ok(created);
        }

        // ========= UPDATE =========

        [HttpPut("{id:long}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật đăng ký sản phẩm (multipart/form-data)")]
        [EndpointDescription("Form-data: Data = ProductRegistrationUpdateDTO; ManualFile (tùy chọn); Images[] (ảnh cần thêm); RemoveImagePublicIds[] (ảnh cần xoá).")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Update(
            ulong id,
            [FromForm] UpdateForm req,
            CancellationToken ct = default)
        {
            //if (id != req.Data.Id) return BadRequest("Id không khớp.");

            // Upload manual (nếu có)
            string? manualUrl = null;
            string? manualPublicUrl = null;
            if (req.ManualFile is not null)
            {
                var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                if (up is not null)
                {
                    manualUrl = up.Url;
                    manualPublicUrl = up.PublicUrl;
                }
            }

            // Upload ảnh thêm (nếu có)
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

            //add certificates
            var addCertificates = new List<MediaLinkItemDTO>();
            if (req.Certificate is { Count: > 0 })
            {
                var ups = await _cloud.UploadManyAsync(req.Certificate, "product-registrations/certificates", ct);
                addCertificates = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();
            }

            var removedImages = req.RemoveImagePublicIds ?? new List<string>();
            var removedCerts = req.RemoveCertificatePublicIds ?? new List<string>();

            try
            {
                var updated = await _service.UpdateAsync(req.Data, manualUrl, manualPublicUrl, addImages, addCertificates, removedImages, removedCerts, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Đơn đăng ký không tồn tại.");
            }
        }

        // ========= CHANGE STATUS / DELETE =========

        [HttpPatch("{id:long}/status")]
        [EndpointSummary("Duyệt / Từ chối đơn đăng ký")]
        [EndpointDescription("Thay đổi trạng thái Pending | Approved | Rejected. Nếu Approved, service sẽ tự xử lý copy dữ liệu sang Product.")]
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
        [EndpointDescription("Xoá bản ghi và các MediaLinks liên quan.")]
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
    }
}
