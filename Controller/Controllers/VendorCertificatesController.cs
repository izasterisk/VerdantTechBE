
using BLL.DTO.MediaLink;
using BLL.DTO.VendorCertificate;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/vendor-certificates")]
    public sealed class VendorCertificatesController : ControllerBase
    {
        private readonly IVendorCertificateService _svc;
        public VendorCertificatesController(IVendorCertificateService svc) { _svc = svc; }


        [EndpointSummary("Tạo nhiều VendorCertificate (1 file = 1 chứng chỉ)")]
        [EndpointDescription("Danh sách file trong addCertificates; mỗi file tạo 1 certificate trạng thái Pending")]
        [HttpPost("bulk")]
        [Consumes("multipart/form-data")]
        //[SwaggerOperation(Summary = "Tạo nhiều VendorCertificate (bulk)", Description = "Danh sách file trong addCertificates; mỗi file tạo 1 certificate trạng thái Pending.")]
        public Task<IReadOnlyList<VendorCertificateResponseDTO>> CreateBulk(
        [FromForm] VendorCertificateCreateDTO dto,
        [FromForm] List<MediaLinkItemDTO> addCertificates,
        CancellationToken ct)
        => _svc.CreateAsync(dto, addCertificates ?? new(), ct);


        [EndpointSummary("Danh sách chứng chỉ theo Vendor (có phân trang)")]
        [EndpointDescription("Trả về danh sách VendorCertificate của 1 vendorId, kèm trạng thái và thời gian xác minh nếu có")]
        [HttpGet]
        //[SwaggerOperation(Summary = "Danh sách chứng chỉ theo Vendor (có phân trang)", Description = "Trả về danh sách VendorCertificate của 1 vendorId, kèm trạng thái và thời gian xác minh nếu có.")]
        public Task<List<VendorCertificateResponseDTO>> GetAllByVendor([FromQuery] ulong vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => _svc.GetAllByVendorAsync(vendorId, page, pageSize, ct);


        [EndpointSummary("Lấy chứng chỉ theo Id")]
        [EndpointDescription("Trả về thông tin VendorCertificate theo Id")]
        [HttpGet("{id}")]
        //[SwaggerOperation(Summary = "Lấy chứng chỉ theo Id", Description = "Trả về thông tin VendorCertificate theo Id.")]
        public Task<VendorCertificateResponseDTO?> GetById([FromRoute] ulong id, CancellationToken ct)
        => _svc.GetByIdAsync(id, ct);


        [EndpointSummary("Duyệt/Đổi trạng thái chứng chỉ")]
        [EndpointDescription("Approved / Rejected / Pending; nếu Reject cần truyền RejectionReason")]
        [HttpPatch("status")]
        //[SwaggerOperation(Summary = "Duyệt/Đổi trạng thái chứng chỉ", Description = "Approved / Rejected / Pending; nếu Reject cần truyền RejectionReason.")]
        public Task<VendorCertificateResponseDTO> ChangeStatus([FromBody] VendorCertificateChangeStatusDTO dto, CancellationToken ct)
        => _svc.ChangeStatusAsync(dto, ct);


        [EndpointSummary("Cập nhật chứng chỉ (thêm/xoá file)")]
        [EndpointDescription("multipart/form-data: dto.*, addCertificates[i].*, removedCertificates[i] (publicId)")]
        [HttpPut("{id:ulong}")]
        [Consumes("multipart/form-data")]
        //[SwaggerOperation(Summary = "Cập nhật chứng chỉ (thêm/xoá file)", Description = "multipart/form-data: dto.*, addCertificates[i].*, removedCertificates[i] (publicId).")]
        public Task<VendorCertificateResponseDTO> Update(
        [FromRoute] ulong id,
        [FromForm] VendorCertificateUpdateDTO dto,
        [FromForm] List<MediaLinkItemDTO> addCertificates,
        [FromForm] List<string> removedCertificates,
        CancellationToken ct)
        {
            if (dto == null) throw new ValidationException("Dto is required");
            if (dto.Id == 0) dto.Id = id; else if (dto.Id != id) throw new ValidationException("Route id và dto id khác nhau");
            return _svc.UpdateAsync(dto, addCertificates ?? new(), removedCertificates ?? new(), ct);
        }


        [EndpointSummary("Xoá chứng chỉ")]
        [EndpointDescription("Xoá VendorCertificate theo Id")]
        [HttpDelete("{id:ulong}")]
        //[SwaggerOperation(Summary = "Xoá chứng chỉ", Description = "Xoá VendorCertificate theo Id.")]
        public Task Delete([FromRoute] ulong id, CancellationToken ct)
        => _svc.DeleteAsync(id, ct);
    }
}