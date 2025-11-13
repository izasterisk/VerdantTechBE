using BLL.DTO.MediaLink;
using BLL.DTO.VendorCertificate;
using BLL.Interfaces;
using Infrastructure.Cloudinary;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorCertificateController : ControllerBase
    {
        private readonly IVendorCertificateService _service;
        private readonly ICloudinaryService _cloudinary;

        public VendorCertificateController(
            IVendorCertificateService service,
            ICloudinaryService cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

       
        [HttpGet("vendor/{vendorId}")]
        [EndpointSummary("Get all certificates of a vendor with pagination.")]
        [EndpointDescription("Fetches a paginated list of vendor certificates belonging to a specific vendor.")]
        public async Task<IActionResult> GetByVendor(
            ulong vendorId,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _service.GetAllByVendorIdAsync(vendorId, page, pageSize, ct);
            return Ok(result);
        }

        
        [HttpGet("{id}")]
        [EndpointSummary("Get vendor certificate details by ID.")]
        [EndpointDescription("Fetches full information of a vendor certificate including verification metadata.")]
        public async Task<IActionResult> GetById(ulong id, CancellationToken ct = default)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result != null ? Ok(result) : NotFound();
        }

       
        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Create multiple vendor certificates with file uploads.")]
        [EndpointDescription("Uploads certificate files to Cloudinary and creates corresponding certificates by index (each file matches 1 certificate item).")]
        public async Task<IActionResult> Create(
            [FromForm] VendorCertificateCreateDto dto,
            [FromForm] List<IFormFile> certificateFiles,
            CancellationToken ct = default)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Danh sách Items không được rỗng.");

            if (certificateFiles == null || certificateFiles.Count == 0)
                return BadRequest("Phải upload ít nhất 1 file.");

            if (dto.Items.Count != certificateFiles.Count)
                return BadRequest("Số Items và số file phải bằng nhau.");

            var uploadResults = await _cloudinary.UploadManyAsync(
                certificateFiles,
                folder: "vendor-certificates",
                ct: ct);

            var mediaList = uploadResults.Select((u, idx) => new MediaLinkItemDTO
            {
                ImagePublicId = u.PublicId,
                ImageUrl = u.PublicUrl,
                Purpose = "certificate",
                SortOrder = idx
            }).ToList();

            var result = await _service.CreateAsync(dto, mediaList, ct);
            return Ok(result);
        }

        
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Update vendor certificate information.")]
        [EndpointDescription("Updates certificate metadata and handles adding/removing certificate files.")]
        public async Task<IActionResult> Update(
            ulong id,
            [FromForm] VendorCertificateUpdateDTO dto,
            [FromForm] List<IFormFile>? newFiles,
            [FromForm] List<string>? removedCertificates,
            CancellationToken ct = default)
        {
            dto.Id = id;

            
            var addMedias = new List<MediaLinkItemDTO>();

            if (newFiles != null && newFiles.Any())
            {
                var uploaded = await _cloudinary.UploadManyAsync(
                    newFiles,
                    folder: "vendor-certificates",
                    ct: ct);

                addMedias = uploaded.Select((u, idx) => new MediaLinkItemDTO
                {
                    ImagePublicId = u.PublicId,
                    ImageUrl = u.PublicUrl,
                    Purpose = "certificate",
                    SortOrder = idx
                }).ToList();
            }

            removedCertificates ??= new List<string>();

            try
            {
                var result = await _service.UpdateAsync(dto, addMedias, removedCertificates, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        
        [HttpDelete("{id}")]
        [EndpointSummary("Delete a vendor certificate.")]
        [EndpointDescription("Deletes a vendor certificate entry. File deletion handled in repository or via Cloudinary service.")]
        public async Task<IActionResult> Delete(ulong id, CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

      
        [HttpPatch("{id}/change-status")]
        [EndpointSummary("Change the status of a vendor certificate.")]
        [EndpointDescription("Updates certificate approval status, verification information, and rejection reason where applicable.")]
        public async Task<IActionResult> ChangeStatus(
            ulong id,
            [FromBody] VendorCertificateChangeStatusDTO dto,
            CancellationToken ct = default)
        {
            dto.Id = id;

            try
            {
                var result = await _service.ChangeStatusAsync(dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
