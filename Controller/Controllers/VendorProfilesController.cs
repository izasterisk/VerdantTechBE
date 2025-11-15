using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BLL.Interfaces;
using BLL.DTO.VendorProfile;
using BLL.DTO.MediaLink;
using Swashbuckle.AspNetCore.Annotations;

namespace Controller.Controllers

{
    [ApiController]
    [Route("api/vendor-profiles")]
    public sealed class VendorProfilesController : ControllerBase
    {
        private readonly IVendorProfileService _svc;
        public VendorProfilesController(IVendorProfileService svc) { _svc = svc; }


        [EndpointSummary("Tạo VendorProfile kèm chứng chỉ ban đầu (1 file = 1 chứng chỉ)")]
        [EndpointDescription("Nhận multipart/form-data: dto.* và addCertificates[i].*; nếu có files, mỗi file sẽ tạo 1 VendorCertificate trạng thái Pending.")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        //[SwaggerOperation(Summary = "Tạo VendorProfile (có thể kèm chứng chỉ)", Description = "Nhận multipart/form-data: dto.* và addCertificates[i].*; nếu có files, mỗi file sẽ tạo 1 VendorCertificate trạng thái Pending.")]
        public Task<VendorProfileResponseDTO> Create(
        [FromForm] VendorProfileCreateDTO dto,
        [FromForm] List<MediaLinkItemDTO> addCertificates,
        CancellationToken ct)
        => _svc.CreateAsync(dto, addCertificates ?? new(), ct);


        [EndpointSummary("Lấy VendorProfile theo Id")]
        [EndpointDescription("Trả về thông tin hồ sơ Vendor")]
        [HttpGet("{id}")]
        //[SwaggerOperation(Summary = "Lấy VendorProfile theo Id", Description = "Trả về thông tin hồ sơ Vendor.")]
        public Task<VendorProfileResponseDTO?> GetById([FromRoute] ulong id, CancellationToken ct)
        => _svc.GetByIdAsync(id, ct);


        [EndpointSummary("Danh sách VendorProfile (có phân trang)")]
        [EndpointDescription("Trả về danh sách VendorProfile theo trang, mặc định page=1, pageSize=20")]
        [HttpGet]
        //[SwaggerOperation(Summary = "Danh sách VendorProfile (có phân trang)", Description = "Trả về danh sách VendorProfile theo trang, mặc định page=1, pageSize=20.")]
        public Task<List<VendorProfileResponseDTO>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => _svc.GetAllAsync(page, pageSize, ct);


        [EndpointSummary("Cập nhật VendorProfile")]
        [EndpointDescription("Cập nhật các trường cơ bản: CompanyName, Slug, BRN, địa chỉ…")]
        [HttpPut("{id}")]
        //[SwaggerOperation(Summary = "Cập nhật VendorProfile", Description = "Cập nhật các trường cơ bản: CompanyName, Slug, BRN, địa chỉ…")]
        public Task<VendorProfileResponseDTO> Update([FromRoute] ulong id, [FromBody] VendorProfileUpdateDTO dto, CancellationToken ct)
        => _svc.UpdateAsync(id, dto, ct);


        [EndpointSummary("Xoá VendorProfile")]
        [EndpointDescription("Xoá hồ sơ Vendor theo Id")]
        [HttpDelete("{id}")]
        //[SwaggerOperation(Summary = "Xoá VendorProfile", Description = "Xoá hồ sơ Vendor theo Id.")]
        public Task Delete([FromRoute] ulong id, CancellationToken ct)
        => _svc.DeleteAsync(id, ct);
    }
}