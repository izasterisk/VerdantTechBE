using System.Net;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductCertificate;
using BLL.Helpers.CertificateFileHelper;   
using BLL.Helpers;   
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Cloudinary;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCertificateController : BaseController
    {

        private readonly IProductCertificateService _productCertificate;
        private readonly ICloudinaryService _cloudinary;
        public ProductCertificateController(IProductCertificateService productCertificate,ICloudinaryService cloudinary) 
        { 
            _productCertificate = productCertificate; 
            _cloudinary = cloudinary;
        }

        public class CreateCertDataItemForm
        {
            public string CertificationCode { get; set; } = null!;
            public string CertificationName { get; set; } = null!;
        }

        public class CreateCertificatesIndexedUploadForm
        {
            public long ProductId { get; set; }

            //public List<CreateCertDataItemForm> Data { get; set; } = new();
            [FromForm] public List<string> CertificationCode { get; set; } = new();
            [FromForm] public List<string> CertificationName { get; set; } = new();
            public List<IFormFile> Files { get; set; } = new();   
        }



        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<APIResponse>> CreateBatch([FromForm] CreateCertificatesIndexedUploadForm form)
        {
            // Validate mềm + hỗ trợ 1 meta + N files
            if (form.Files == null || form.Files.Count == 0)
                return ErrorResponse("Thiếu files (PDF)");
            if (form.CertificationCode == null || form.CertificationName == null)
                return ErrorResponse("Thiếu data (code/name)");

            //// Nếu chỉ có 1 meta nhưng nhiều file, tự nhân bản meta cho khớp số file
            //if (form.Data.Count == 1 && form.Files.Count > 1)
            //{
            //    var first = form.Data[0];
            //    while (form.Data.Count < form.Files.Count)
            //        form.Data.Add(new CreateCertDataItemForm
            //        {
            //            CertificationCode = first.CertificationCode,
            //            CertificationName = first.CertificationName
            //        });
            //}

            // Sau khi “fill”, nếu vẫn lệch → báo lỗi
            if (form.CertificationCode.Count != form.CertificationName.Count || form.CertificationCode.Count != form.Files.Count)
                return ErrorResponse("Số lượng data và files phải bằng nhau");

            var all = new List<ProductCertificateResponseDTO>();

            for (int i = 0; i < form.Files.Count; i++)
            {
                var file = form.Files[i];
                var code = form.CertificationCode[i];
                var name = form.CertificationName[i];

                var up = await _cloudinary.UploadAsync(file, "product-certificates", GetCancellationToken());

                var add = new List<MediaLinkItemDTO>
        {
            new()
            {
                ImagePublicId = up.PublicId,
                ImageUrl      = up.PublicUrl,
                Purpose       = "CertificatePdf",
                SortOrder     = 1
            }
        };

                var dto = new ProductCertificateCreateDTO
                {
                    ProductId = (ulong)form.ProductId,
                    CertificationCode = code,
                    CertificationName = name
                };

                var created = await _productCertificate.CreateAsync(dto, add, GetCancellationToken());
                all.AddRange(created);
            }

            return SuccessResponse(all, HttpStatusCode.Created);
        }


        [HttpGet]
        [EndpointSummary("Lấy danh sách chứng nhận")]
        public async Task<ActionResult<APIResponse>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vr = ValidateModel();
            if (vr != null) return vr;

            try
            {
                var result = await _productCertificate.GetAllAsync(page, pageSize, GetCancellationToken());
                return SuccessResponse(result, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        
        [HttpGet("by-product/{productId:long}")]
        [EndpointSummary("Lấy danh sách chứng nhận của sản phẩm theo ProductId")]
        public async Task<ActionResult<APIResponse>> GetByProduct([FromRoute] ulong productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vr = ValidateModel();
            if (vr != null) return vr;

            try
            {
                var result = await _productCertificate.GetByProductIdAsync(productId, page, pageSize, GetCancellationToken());
                return SuccessResponse(result, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id:long}")]
        [EndpointSummary("Chi tiết chứng nhận theo Id")]
        public async Task<ActionResult<APIResponse>> GetById([FromRoute] ulong id)
        {
            var vr = ValidateModel();
            if (vr != null) return vr;

            try
            {
                var result = await _productCertificate.GetByIdAsync(id, GetCancellationToken());
                return result is null
                    ? ErrorResponse("Không tìm thấy chứng nhận", HttpStatusCode.NotFound)
                    : SuccessResponse(result, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo chứng nhận: 1 code/name +  PDF ")]
        public async Task<ActionResult<APIResponse>> Create(
            [FromForm] ProductCertificateCreateDTO dto,
            [FromForm] List<IFormFile> files)
        {
            var vr = ValidateModel(); if (vr != null) return vr;

            if (files == null || files.Count == 0) return ErrorResponse("Thiếu files (PDF)");
            try
            {
                var list = new List<ProductCertificateResponseDTO>();

                foreach (var file in files)
                {
                    var up = await _cloudinary.UploadAsync(file, "product-certificates", GetCancellationToken());
                    var add = new List<MediaLinkItemDTO>
                    {
                        new MediaLinkItemDTO
                        {
                            ImagePublicId = up.PublicId,
                            ImageUrl      = up.PublicUrl,
                            Purpose       = "CertificatePdf",
                            SortOrder     = 1
                        }
                    };

                    var created = await _productCertificate.CreateAsync(dto, add, GetCancellationToken());
                    list.AddRange(created); 
                }

                return SuccessResponse(list, HttpStatusCode.Created);
            }
            catch (Exception ex) { return HandleException(ex); }
        }



        // PUT: api/ProductCertificate/{id}
        // Body (application/json):
        // { "data": {id?, certificationCode, certificationName}, "addFiles": [...], "removeFilePublicIds": [...] }
        [HttpPut("{id:long}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật chứng nhận; có thể kèm file mới. Nếu muốn xoá file cũ, truyền removedFilePublicIds.")]
        public async Task<ActionResult<APIResponse>> Update(
            [FromRoute] long id,
            [FromForm] ProductCertificateUpdateDTO form,
            [FromForm] List<IFormFile>? addFiles,
            [FromForm] List<string>? removedFilePublicIds)
        {
            var vr = ValidateModel(); if (vr != null) return vr;
            if (id < 0) return ErrorResponse("id không hợp lệ");

            form.Id = (ulong)id;

            var add = new List<MediaLinkItemDTO>();
            if (addFiles != null && addFiles.Count > 0)
            {
                foreach (var file in addFiles)
                {
                    var up = await _cloudinary.UploadAsync(file, "product-certificates", GetCancellationToken());
                    add.Add(new MediaLinkItemDTO
                    {
                        ImagePublicId = up.PublicId,
                        ImageUrl = up.PublicUrl,
                        Purpose = "CertificatePdf",
                        SortOrder = 1
                    });
                }
            }

            var updated = await _productCertificate.UpdateAsync(
                form,
                add,
                removedFilePublicIds ?? new List<string>(),
                GetCancellationToken());

            return SuccessResponse(updated, HttpStatusCode.OK);
        }

        // PATCH: api/ProductCertificate/status/{id}
        [HttpPatch("status/{id:long}")]
        [Consumes("application/json")]
        [EndpointSummary("Duyệt / Từ chối chứng nhận của sản phẩm")]
        public async Task<ActionResult<APIResponse>> ChangeStatus([FromRoute] long id, [FromBody] ProductCertificateChangeStatusDTO body)
        {
            var vr = ValidateModel(); if (vr != null) return vr;
            if (id < 0) return ErrorResponse("id không hợp lệ");

            body.Id = (ulong)id;
            try
            {
                var ok = await _productCertificate.ChangeStatusAsync(body, GetCancellationToken());
                return ok ? SuccessResponse(null, HttpStatusCode.NoContent)
                          : ErrorResponse("Không tìm thấy chứng nhận", HttpStatusCode.NotFound);
            }
            catch (Exception ex) { return HandleException(ex); }
        }



        [HttpDelete("{id}")]
        [EndpointSummary("Xoá chứng nhận của sản phẩm theo Id")]
        public async Task<ActionResult<APIResponse>> Delete([FromRoute] long id)
        {
            if (id < 0) return ErrorResponse("id không hợp lệ");
            try
            {
                var ok = await _productCertificate.DeleteAsync((ulong)id, GetCancellationToken());
                return ok ? SuccessResponse(null, HttpStatusCode.NoContent)
                          : ErrorResponse("Không tìm thấy chứng nhận", HttpStatusCode.NotFound);
            }
            catch (Exception ex) { return HandleException(ex); }
        }

        // ====== request models ======

        public record CreateCertificateWithFilesRequest(ProductCertificateCreateDTO Data, List<MediaLinkItemDTO> Files);
        public record UpdateCertificateWithFilesRequest(ProductCertificateUpdateDTO Data, List<MediaLinkItemDTO>? AddFiles, List<string>? RemoveFilePublicIds);
        public record ChangeCertStatusRequest(ProductCertificateStatus Status, string? RejectionReason, ulong? VerifiedBy);
    }
}
