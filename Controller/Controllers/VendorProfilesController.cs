using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BLL.DTO.MediaLink;
using BLL.DTO.VendorProfile;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using CloudinaryDotNet.Actions;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorProfilesController : ControllerBase
    {
        private readonly IVendorProfileService _vendorProfileService;
        private readonly ICloudinaryService _cloudinary;

        public VendorProfilesController(IVendorProfileService vendorProfileService, ICloudinaryService cloudinary)
        {
            _vendorProfileService = vendorProfileService;
            _cloudinary = cloudinary;
        }

        // ---------------------------
        // GET ALL (paging)
        // ---------------------------
        [HttpGet]
        [EndpointSummary("Lấy danh sách vendor theo phân trang")]
        [EndpointDescription("Trả về danh sách vendor profile đang active kèm thông tin user, address và media chứng chỉ.")]
        public async Task<ActionResult<List<VendorProfileResponseDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var result = await _vendorProfileService.GetAllAsync(page, pageSize, ct);
            return Ok(result);
        }

        // ---------------------------
        // GET BY ID
        // ---------------------------
        [HttpGet("{id}")]
        [EndpointSummary("Lấy chi tiết VendorProfile theo Id")]
        [EndpointDescription("Bao gồm thông tin vendor, user, địa chỉ công ty và danh sách media chứng chỉ.")]
        public async Task<ActionResult<VendorProfileResponseDTO>> GetById(
            [FromRoute] ulong id,
            CancellationToken ct = default)
        {
            var result = await _vendorProfileService.GetByIdAsync(id, ct);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ---------------------------
        // GET BY USER ID
        // ---------------------------
        [HttpGet("by-user/{userId}")]
        [EndpointSummary("Lấy VendorProfile theo UserId (vendorId)")]
        [EndpointDescription("Dùng khi cần tìm vendor theo tài khoản user.")]
        public async Task<ActionResult<VendorProfileResponseDTO>> GetByUserId(
            [FromRoute] ulong userId,
            CancellationToken ct = default)
        {
            var result = await _vendorProfileService.GetByUserIdAsync(userId, ct);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ---------------------------
        // CREATE (multipart/form-data)
        // ---------------------------
        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo mới VendorProfile (vendor đăng ký)")]
        [EndpointDescription(
            "Tạo User (Role = Vendor) → VendorProfile → Address (nếu có) → UserAddress → VendorCertificate (Pending) + Media cho từng chứng chỉ."
        )]
        public async Task<ActionResult<VendorProfileResponseDTO>> Create(
            [FromForm] VendorProfileCreateDTO dto,
            [FromForm] List<IFormFile> files,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (files == null || files.Count == 0)
                return BadRequest("Không có tệp chứng chỉ nào được cung cấp.");

            // Upload tất cả file CHỈ 1 LẦN
            var uploadResults = await _cloudinary.UploadManyAsync(files, "vendor-certificates", ct);

            if (uploadResults == null || uploadResults.Count != files.Count)
                return StatusCode(500, "Upload file chứng chỉ thất bại.");

            // Map kết quả upload sang MediaLink để truyền xuống service
            var mediaLinks = uploadResults
                .Select((u, index) => new MediaLink
                {
                    ImagePublicId = u.PublicId,
                    ImageUrl = u.PublicUrl,
                    Purpose = MediaPurpose.VendorCertificatesPdf,
                    SortOrder = (ushort)index,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            try
            {
                var created = await _vendorProfileService.CreateAsync(
                    dto,
                    mediaLinks,
                    ct);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Lỗi hệ thống, vui lòng thử lại sau." });
            }

        }

        // ---------------------------
        // UPDATE
        // ---------------------------
        [HttpPut("{id}")]
        [EndpointSummary("Cập nhật VendorProfile")]
        [EndpointDescription("Cập nhật VendorProfile + thông tin User + địa chỉ hiện tại của vendor.")]
        public async Task<IActionResult> Update(
            [FromRoute] ulong id,
            [FromBody] VendorProfileUpdateDTO dto,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Id == 0)
                dto.Id = id;
            else if (dto.Id != id)
                return BadRequest("Id trong route và body không khớp");

            await _vendorProfileService.UpdateAsync(dto, ct);
            return NoContent();
        }

        // ---------------------------
        // DELETE VendorProfile (hard delete)
        // ---------------------------
        [HttpDelete("{id}")]
        [EndpointSummary("Xóa VendorProfile (hard delete)")]
        [EndpointDescription("Không xóa user. Chỉ xóa thông tin vendor profile khỏi hệ thống.")]
        public async Task<IActionResult> Delete(
            [FromRoute] ulong id,
            CancellationToken ct = default)
        {
            await _vendorProfileService.DeleteAsync(id, ct);
            return NoContent();
        }

        // ---------------------------
        // SOFT DELETE USER ACCOUNT
        // ---------------------------
        [HttpDelete("account/{userId}")]
        [EndpointSummary("Soft delete tài khoản vendor")]
        [EndpointDescription("Chỉ set User.Status = Inactive và DeletedAt, không xóa dữ liệu.")]
        public async Task<IActionResult> SoftDeleteAccount(
            [FromRoute] ulong userId,
            CancellationToken ct = default)
        {
            await _vendorProfileService.SoftDeleteAccountAsync(userId, ct);
            return NoContent();
        }

        // ---------------------------
        // APPROVE
        // ---------------------------
        [HttpPost("{id}/approve")]
        [EndpointSummary("Duyệt vendor profile")]
        [EndpointDescription(
            "Duyệt VendorProfile → update User (IsVerified) → update VendorProfile (VerifiedAt/By) → " +
            "duyệt tất cả VendorCertificate → gửi email xác nhận duyệt cho vendor."
        )]
        public async Task<IActionResult> Approve(
            [FromRoute] ulong id,
            [FromBody] VendorProfileApproveDTO dto,
            CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Dữ liệu duyệt không hợp lệ");

            dto.Id = id;

            await _vendorProfileService.ApproveAsync(dto, ct);
            return NoContent();
        }

        // ---------------------------
        // REJECT
        // ---------------------------
        [HttpPost("{id}/reject")]
        [EndpointSummary("Từ chối vendor profile")]
        [EndpointDescription(
            "Từ chối VendorProfile → update User (IsVerified=false) → update VendorProfile VerifiedAt/By → " +
            "update tất cả VendorCertificate = Rejected + lưu lý do → gửi email báo từ chối."
        )]
        public async Task<IActionResult> Reject(
            [FromRoute] ulong id,
            [FromBody] VendorProfileRejectDTO dto,
            CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Dữ liệu từ chối không hợp lệ");

            dto.Id = id;

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
                return BadRequest("RejectionReason là bắt buộc");

            await _vendorProfileService.RejectAsync(dto, ct);
            return NoContent();
        }
    }
}
