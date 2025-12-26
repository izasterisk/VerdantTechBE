using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Swashbuckle.AspNetCore.Annotations;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.Services;
using BLL.Helpers.Excel;
using BLL.DTO.Product;
using OfficeOpenXml;
using Microsoft.Extensions.Logging;
using DAL.Data;
using DAL.Data.Models;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductRegistrationsController : BaseController
{
    private readonly IProductRegistrationService _service;
    private readonly ICloudinaryService _cloud;
    private readonly ILogger<ProductRegistrationsController> _logger;
    private readonly ProductRegistrationImportService _importService;
    private readonly VerdantTechDbContext _db;

    public ProductRegistrationsController(
        IProductRegistrationService service,
        ICloudinaryService cloud,
        ILogger<ProductRegistrationsController> logger,
        ProductRegistrationImportService importService,
        VerdantTechDbContext db)
    {
        _service = service;
        _cloud = cloud;
        _logger = logger;
        _importService = importService;
        _db = db;
    }


    // 🔧 Logging helper
    private void LogDict(string label, Dictionary<string, object>? dict)
    {
        try
        {
            var json = dict is null ? "null" : JsonSerializer.Serialize(dict);
            _logger.LogInformation("{Label}: {Json}", label, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Serialize specs failed in controller");
        }
    }

        // =====================================================================
        // 📌 READ ENDPOINTS
        // =====================================================================

        [HttpGet]
        [EndpointSummary("Danh sách đăng ký sản phẩm (phân trang)")]
        [EndpointDescription("Trả về danh sách product registrations bao gồm hình ảnh, file manual, certificate và thông tin chi tiết.")]
        public async Task<ActionResult<PagedResponse<ProductRegistrationReponseDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await _service.GetAllAsync(page, pageSize, ct);
            return Ok(res);
        }


        [HttpGet("{id}")]
        [EndpointSummary("Lấy chi tiết đăng ký sản phẩm theo ID")]
        [EndpointDescription("Trả về thông tin chi tiết của một product registration, bao gồm images, specifications và certificate.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> GetById(
            ulong id,
            CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, ct);
            return item is null ? NotFound("Đơn đăng ký không tồn tại.") : Ok(item);
        }


        [HttpGet("vendor/{vendorId}")]
        [EndpointSummary("Lấy danh sách đăng ký theo Vendor")]
        [EndpointDescription("Trả về tất cả đăng ký sản phẩm của một Vendor kèm phân trang.")]
        public async Task<ActionResult<PagedResponse<ProductRegistrationReponseDTO>>> GetByVendor(
            ulong vendorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await _service.GetByVendorAsync(vendorId, page, pageSize, ct);
            return Ok(res);
        }


        // =====================================================================
        // 📌 CREATE ENDPOINT
        // =====================================================================

        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo mới Product Registration")]
        [EndpointDescription("Tạo đăng ký sản phẩm mới bao gồm: hình ảnh sản phẩm, file manual PDF, certificates PDF, specifications và thông tin mô tả.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Create(
            [FromForm] CreateForm req,
            CancellationToken ct = default)
        {
            // Track uploaded files để cleanup nếu có lỗi
            string? manualPublicId = null;
            var uploadedImagePublicIds = new List<string>();
            var uploadedCertPublicIds = new List<string>();

            try
            {
                FillSpecsFromForm(req.Data, Request.Form);

                // Upload manual
                string? manualUrl = null, manualPublicUrl = null;
                if (req.ManualFile is not null)
                {
                    var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                    if (up is not null)
                    {
                        manualUrl = up.Url;
                        manualPublicUrl = up.PublicUrl;
                        manualPublicId = up.PublicId;
                    }
                }

                // Upload product images
                var imagesDto = new List<MediaLinkItemDTO>();
                if (req.Images is { Count: > 0 })
                {
                    try
                    {
                        var ups = await _cloud.UploadManyAsync(req.Images, "product-registrations/images", ct);
                        imagesDto = ups.Select((x, i) => new MediaLinkItemDTO
                        {
                            ImagePublicId = x.PublicId,
                            ImageUrl = x.Url,
                            Purpose = "none",
                            SortOrder = i + 1
                        }).ToList();
                        uploadedImagePublicIds.AddRange(ups.Select(x => x.PublicId));
                    }
                    catch (Exception uploadEx)
                    {
                        // Cleanup files đã upload trước đó nếu có
                        foreach (var publicId in uploadedImagePublicIds)
                        {
                            try
                            {
                                await _cloud.DeleteAsync(publicId, ct);
                            }
                            catch { /* Ignore cleanup errors */ }
                        }
                        throw new InvalidOperationException($"Lỗi khi upload hình ảnh sản phẩm: {uploadEx.Message}", uploadEx);
                    }
                }

                // Upload certificate files
                var certDtos = new List<MediaLinkItemDTO>();
                if (req.Certificate is { Count: > 0 })
                {
                    try
                    {
                        // Chỉ chấp nhận file PDF
                        var pdfFiles = req.Certificate.Where(f =>
                            string.Equals(f.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                            f.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        ).ToList();

                        if (pdfFiles.Count != req.Certificate.Count)
                        {
                            throw new InvalidOperationException("Chỉ chấp nhận file PDF cho chứng chỉ. Vui lòng kiểm tra lại các file đã chọn.");
                        }

                        var ups = await _cloud.UploadManyAsync(pdfFiles, "product-registrations/certificates", ct);

                        certDtos = ups.Select((x, i) => new MediaLinkItemDTO
                        {
                            ImagePublicId = x.PublicId,
                            ImageUrl = x.PublicUrl,
                            Purpose = "certificatepdf",
                            SortOrder = i + 1
                        }).ToList();
                        uploadedCertPublicIds.AddRange(ups.Select(x => x.PublicId));
                    }
                    catch (Exception uploadEx)
                    {
                        // Cleanup files đã upload trước đó nếu có
                        foreach (var publicId in uploadedCertPublicIds)
                        {
                            try
                            {
                                await _cloud.DeleteAsync(publicId, ct);
                            }
                            catch { /* Ignore cleanup errors */ }
                        }
                        // Cleanup product images đã upload nếu có
                        foreach (var publicId in uploadedImagePublicIds)
                        {
                            try
                            {
                                await _cloud.DeleteAsync(publicId, ct);
                            }
                            catch { /* Ignore cleanup errors */ }
                        }
                        throw new InvalidOperationException($"Lỗi khi upload file chứng chỉ: {uploadEx.Message}", uploadEx);
                    }
                }

                var created = await _service.CreateAsync(
                    req.Data,
                    manualUrl,
                    manualPublicUrl,
                    imagesDto,
                    certDtos,
                    ct
                );

                return Ok(created);
            }
            catch (Exception ex)
            {
                // Cleanup: Xóa các files đã upload trên Cloudinary nếu có lỗi
                _logger.LogWarning(ex, "Error creating product registration. Cleaning up uploaded files.");

                try
                {
                    // Xóa manual file
                    if (!string.IsNullOrEmpty(manualPublicId))
                    {
                        await _cloud.DeleteAsync(manualPublicId, ct);
                    }

                    // Xóa product images
                    foreach (var publicId in uploadedImagePublicIds)
                    {
                        await _cloud.DeleteAsync(publicId, ct);
                    }

                    // Xóa certificate files
                    foreach (var publicId in uploadedCertPublicIds)
                    {
                        await _cloud.DeleteAsync(publicId, ct);
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Error during cleanup of uploaded files");
                }

                throw; // Re-throw original exception
            }
        }


        // =====================================================================
        // 📌 UPDATE ENDPOINT
        // =====================================================================

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật Product Registration")]
        [EndpointDescription("Cập nhật thông tin đăng ký sản phẩm, thêm/xoá hình ảnh, thêm/xoá certificate, cập nhật manual và specifications.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> Update(
            ulong id,
            [FromForm] UpdateForm req,
            CancellationToken ct = default)
        {
            FillSpecsFromForm(req.Data, Request.Form);

            string? manualUrl = null, manualPublicUrl = null;
            if (req.ManualFile is not null)
            {
                var up = await _cloud.UploadAsync(req.ManualFile, "product-registrations/manuals", ct);
                if (up is not null)
                {
                    manualUrl = up.Url;
                    manualPublicUrl = up.PublicUrl;
                }
            }

            // Add images
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

            // Add certificates
            var addCerts = new List<MediaLinkItemDTO>();
            if (req.Certificate is { Count: > 0 })
            {
                // Chỉ chấp nhận file PDF
                var pdfFiles = req.Certificate.Where(f =>
                    string.Equals(f.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    f.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                if (pdfFiles.Count != req.Certificate.Count)
                {
                    throw new InvalidOperationException("Chỉ chấp nhận file PDF cho chứng chỉ. Vui lòng kiểm tra lại các file đã chọn.");
                }

                var ups = await _cloud.UploadManyAsync(pdfFiles, "product-registrations/certificates", ct);
                addCerts = ups.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.PublicUrl,
                    Purpose = "certificatepdf",
                    SortOrder = i + 1
                }).ToList();
            }

            var updated = await _service.UpdateAsync(
                req.Data,
                manualUrl,
                manualPublicUrl,
                addImages,
                addCerts,
                req.RemoveImagePublicIds ?? new List<string>(),
                req.RemoveCertificatePublicIds ?? new List<string>(),
                ct
            );

            return Ok(updated);
        }


        // =====================================================================
        // 📌 CHANGE STATUS ENDPOINT
        // =====================================================================

        [HttpPatch("{id}/status")]
        [EndpointSummary("Duyệt hoặc từ chối đăng ký")]
        [EndpointDescription("Thay đổi trạng thái của đơn đăng ký sản phẩm: Approved hoặc Rejected. "
            + "Khi được duyệt (Approved), hệ thống sẽ tự động tạo Product và copy toàn bộ media/ certificates.")]
        public async Task<IActionResult> ChangeStatus(
            ulong id,
            [FromBody] ProductRegistrationChangeStatusDTO dto,
            CancellationToken ct = default)
        {
            var ok = await _service.ChangeStatusAsync(id, dto.Status, dto.RejectionReason, dto.ApprovedBy, ct);
            return ok ? NoContent() : NotFound("Đơn đăng ký không tồn tại.");
        }


        // =====================================================================
        // 📌 DELETE ENDPOINT
        // =====================================================================

        [HttpDelete("{id}")]
        [EndpointSummary("Xoá đăng ký sản phẩm")]
        [EndpointDescription("Xoá một product registration theo ID. "
            + "Không xoá Products đã được tạo từ Approved.")]
        public async Task<IActionResult> Delete(ulong id, CancellationToken ct = default)
        {
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound("Đơn đăng ký không tồn tại.");
        }


        // =====================================================================
        // 📌 EXCEL IMPORT ENDPOINTS
        // =====================================================================

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Import ProductRegistrations từ file Excel")]
        [EndpointDescription("Import nhiều ProductRegistration từ file Excel. " +
            "File Excel phải có các cột: CategoryId, ProposedProductCode, ProposedProductName, " +
            "UnitPrice, WeightKg, LengthCm, WidthCm, HeightCm và các trường tùy chọn khác. " +
            "VendorId sẽ được tự động lấy từ user đăng nhập. " +
            "Sau khi import, dùng endpoint POST /{id}/certificates để upload chứng chỉ (bắt buộc). " +
            "Có thể upload hình ảnh và manual file qua endpoint tương ứng.")]
        public async Task<ActionResult<ProductRegistrationImportResponseDTO>> ImportFromExcel(
            [FromForm] ImportExcelForm form,
            CancellationToken ct = default)
        {
            if (form.File == null || form.File.Length == 0)
                return BadRequest("File Excel không được để trống.");

            if (!ExcelHelper.ValidateExcelFormat(form.File.FileName))
                return BadRequest("File phải có định dạng .xlsx hoặc .xls");

            try
            {
                // Tự động lấy VendorId từ user đăng nhập
                var vendorId = GetCurrentUserId();
                
                // Sử dụng OpenReadStream() và đảm bảo dispose đúng cách
                byte[] fileBytes;
                using (var fileStream = form.File.OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream, ct);
                    fileBytes = memoryStream.ToArray();
                }
                
                using var stream = new MemoryStream(fileBytes);
                var result = await _importService.ImportFromExcelAsync(stream, vendorId, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi import ProductRegistrations từ Excel");
                return StatusCode(500, new { error = $"Lỗi khi xử lý file Excel: {ex.Message}" });
            }
        }

        [HttpPost("{id}/images")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật hình ảnh cho ProductRegistration sau khi import")]
        [EndpointDescription("Upload hình ảnh cho ProductRegistration đã được tạo từ import Excel. " +
            "Hình ảnh sẽ được upload lên Cloudinary và liên kết với ProductRegistration.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> UpdateImages(
            ulong id,
            [FromForm] List<IFormFile> images,
            CancellationToken ct = default)
        {
            if (images == null || images.Count == 0)
                return BadRequest("Vui lòng chọn ít nhất một hình ảnh.");

            try
            {
                // Upload images to Cloudinary
                var uploadedImages = await _cloud.UploadManyAsync(images, "product-registrations/images", ct);
                var imageDtos = uploadedImages.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();

                // Get existing ProductRegistration to update
                var existing = await _service.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound("Không tìm thấy ProductRegistration với ID này.");

                // Update with new images (using UpdateAsync with empty remove lists)
                var updateDto = new ProductRegistrationUpdateDTO
                {
                    Id = id,
                    VendorId = existing.VendorId,
                    CategoryId = existing.CategoryId,
                    ProposedProductCode = existing.ProposedProductCode,
                    ProposedProductName = existing.ProposedProductName,
                    Description = existing.Description,
                    UnitPrice = existing.UnitPrice,
                    EnergyEfficiencyRating = existing.EnergyEfficiencyRating,
                    WarrantyMonths = existing.WarrantyMonths,
                    WeightKg = existing.WeightKg,
                    DimensionsCm = new ProductUpdateDTO.DimensionsDTO
                    {
                        Length = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("length", out var len) 
                            ? Convert.ToDecimal(len) : 0,
                        Width = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("width", out var wid) 
                            ? Convert.ToDecimal(wid) : 0,
                        Height = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("height", out var hei) 
                            ? Convert.ToDecimal(hei) : 0
                    }
                };

                var updated = await _service.UpdateAsync(
                    updateDto,
                    manualUrl: null,
                    manualPublicUrl: null,
                    addImages: imageDtos,
                    addCertificates: new List<MediaLinkItemDTO>(),
                    removedImages: new List<string>(),
                    removedCertificates: new List<string>(),
                    ct);

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hình ảnh cho ProductRegistration {Id}", id);
                return StatusCode(500, new { error = $"Lỗi khi cập nhật hình ảnh: {ex.Message}" });
            }
        }

        [HttpPost("{id}/certificates")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Upload certificates cho ProductRegistration sau khi import")]
        [EndpointDescription("Upload file chứng chỉ PDF cho ProductRegistration đã được tạo từ import Excel. " +
            "Files sẽ được upload lên Cloudinary và liên kết với ProductRegistration. " +
            "Hỗ trợ CertificationCode và CertificationName arrays.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> UpdateCertificates(
            ulong id,
            [FromForm] List<IFormFile> certificates,
            [FromForm] List<string>? certificationCodes = null,
            [FromForm] List<string>? certificationNames = null,
            CancellationToken ct = default)
        {
            if (certificates == null || certificates.Count == 0)
                return BadRequest("Vui lòng chọn ít nhất một file chứng chỉ.");

            // Validate: nếu có code hoặc name thì số lượng phải khớp với số files
            if ((certificationCodes != null && certificationCodes.Count != certificates.Count) ||
                (certificationNames != null && certificationNames.Count != certificates.Count))
            {
                return BadRequest("Số lượng mã chứng chỉ và tên chứng chỉ phải khớp với số lượng file.");
            }

            // Validate: nếu có code thì phải có name và ngược lại
            if ((certificationCodes != null && certificationNames == null) ||
                (certificationCodes == null && certificationNames != null))
            {
                return BadRequest("Phải cung cấp cả mã chứng chỉ và tên chứng chỉ hoặc không cung cấp cả hai.");
            }

            try
            {
                // Chỉ chấp nhận file PDF
                var pdfFiles = certificates.Where(f =>
                    string.Equals(f.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    f.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                if (pdfFiles.Count != certificates.Count)
                {
                    return BadRequest("Chỉ chấp nhận file PDF cho chứng chỉ. Vui lòng kiểm tra lại các file đã chọn.");
                }

                // Get existing ProductRegistration
                var existing = await _service.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound("Không tìm thấy ProductRegistration với ID này.");

                // Upload certificates to Cloudinary
                var uploadedCerts = await _cloud.UploadManyAsync(pdfFiles, "product-registrations/certificates", ct);

                // Tạo ProductCertificate với code và name nếu có
                for (int i = 0; i < uploadedCerts.Count; i++)
                {
                    var cert = new ProductCertificate
                    {
                        RegistrationId = existing.Id,
                        ProductId = null,
                        CertificationName = certificationNames != null && i < certificationNames.Count && !string.IsNullOrWhiteSpace(certificationNames[i])
                            ? certificationNames[i].Trim()
                            : $"Chứng chỉ {i + 1}",
                        CertificationCode = certificationCodes != null && i < certificationCodes.Count && !string.IsNullOrWhiteSpace(certificationCodes[i])
                            ? certificationCodes[i].Trim()
                            : $"CERT-{existing.Id}-{i + 1}",
                        Status = ProductCertificateStatus.Pending,
                        UploadedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.ProductCertificates.AddAsync(cert, ct);
                    await _db.SaveChangesAsync(ct);

                    var media = new MediaLink
                    {
                        OwnerType = MediaOwnerType.ProductCertificates,
                        OwnerId = cert.Id,
                        ImagePublicId = uploadedCerts[i].PublicId,
                        ImageUrl = uploadedCerts[i].PublicUrl,
                        Purpose = MediaPurpose.ProductCertificatePdf,
                        SortOrder = i + 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _db.MediaLinks.AddAsync(media, ct);
                    await _db.SaveChangesAsync(ct);
                }

                // Reload để trả về dữ liệu mới nhất
                var updated = await _service.GetByIdAsync(id, ct);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật certificates cho ProductRegistration {Id}", id);
                return StatusCode(500, new { error = $"Lỗi khi cập nhật certificates: {ex.Message}" });
            }
        }

        [HttpPost("{id}/manual")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Upload manual PDF cho ProductRegistration sau khi import")]
        [EndpointDescription("Upload file manual PDF cho ProductRegistration đã được tạo từ import Excel. " +
            "File sẽ được upload lên Cloudinary và liên kết với ProductRegistration.")]
        public async Task<ActionResult<ProductRegistrationReponseDTO>> UpdateManual(
            ulong id,
            [FromForm] UpdateManualForm form,
            CancellationToken ct = default)
        {
            if (form.ManualFile == null || form.ManualFile.Length == 0)
                return BadRequest("Vui lòng chọn file manual.");

            try
            {
                // Upload manual to Cloudinary
                var uploadResult = await _cloud.UploadAsync(form.ManualFile, "product-registrations/manuals", ct);
                if (uploadResult == null)
                {
                    return StatusCode(500, new { error = "Lỗi khi upload file manual." });
                }

                // Get existing ProductRegistration to update
                var existing = await _service.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound("Không tìm thấy ProductRegistration với ID này.");

                // Update with new manual (using UpdateAsync with empty lists)
                var updateDto = new ProductRegistrationUpdateDTO
                {
                    Id = id,
                    VendorId = existing.VendorId,
                    CategoryId = existing.CategoryId,
                    ProposedProductCode = existing.ProposedProductCode,
                    ProposedProductName = existing.ProposedProductName,
                    Description = existing.Description,
                    UnitPrice = existing.UnitPrice,
                    EnergyEfficiencyRating = existing.EnergyEfficiencyRating,
                    WarrantyMonths = existing.WarrantyMonths,
                    WeightKg = existing.WeightKg,
                    DimensionsCm = new ProductUpdateDTO.DimensionsDTO
                    {
                        Length = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("length", out var len) 
                            ? Convert.ToDecimal(len) : 0,
                        Width = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("width", out var wid) 
                            ? Convert.ToDecimal(wid) : 0,
                        Height = existing.DimensionsCm != null && existing.DimensionsCm.TryGetValue("height", out var hei) 
                            ? Convert.ToDecimal(hei) : 0
                    }
                };

                var updated = await _service.UpdateAsync(
                    updateDto,
                    manualUrl: uploadResult.Url,
                    manualPublicUrl: uploadResult.PublicUrl,
                    addImages: new List<MediaLinkItemDTO>(),
                    addCertificates: new List<MediaLinkItemDTO>(),
                    removedImages: new List<string>(),
                    removedCertificates: new List<string>(),
                    ct);

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật manual cho ProductRegistration {Id}", id);
                return StatusCode(500, new { error = $"Lỗi khi cập nhật manual: {ex.Message}" });
            }
        }

        [HttpGet("import/template")]
        [EndpointSummary("Tải template Excel cho ProductRegistration import")]
        [EndpointDescription("Tải file Excel template với các cột mẫu để import ProductRegistration.")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                
                var worksheet = package.Workbook.Worksheets.Add("ProductRegistrations");

                // Header row (VendorId sẽ được tự động lấy từ user đăng nhập)
                worksheet.Cells[1, 1].Value = "CategoryName";
                worksheet.Cells[1, 2].Value = "ProposedProductCode";
                worksheet.Cells[1, 3].Value = "ProposedProductName";
                worksheet.Cells[1, 4].Value = "Description";
                worksheet.Cells[1, 5].Value = "UnitPrice";
                worksheet.Cells[1, 6].Value = "EnergyEfficiencyRating";
                worksheet.Cells[1, 7].Value = "WarrantyMonths";
                worksheet.Cells[1, 8].Value = "WeightKg";
                worksheet.Cells[1, 9].Value = "LengthCm";
                worksheet.Cells[1, 10].Value = "WidthCm";
                worksheet.Cells[1, 11].Value = "HeightCm";
                worksheet.Cells[1, 12].Value = "Specifications";

                // Example row
                worksheet.Cells[2, 1].Value = 1;
                worksheet.Cells[2, 2].Value = "PROD001";
                worksheet.Cells[2, 3].Value = "Sản phẩm mẫu";
                worksheet.Cells[2, 4].Value = "Mô tả sản phẩm";
                worksheet.Cells[2, 5].Value = 100000;
                worksheet.Cells[2, 6].Value = "5";
                worksheet.Cells[2, 7].Value = 12;
                worksheet.Cells[2, 8].Value = 1.5;
                worksheet.Cells[2, 9].Value = 10;
                worksheet.Cells[2, 10].Value = 20;
                worksheet.Cells[2, 11].Value = 30;
                worksheet.Cells[2, 12].Value = "{\"key1\":\"value1\"}";

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "ProductRegistration_Import_Template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo template Excel");
                return StatusCode(500, new { error = $"Lỗi khi tạo template: {ex.Message}" });
            }
        }


        // =====================================================================
        // 📌 FORM MODELS
        // =====================================================================

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

        public sealed class ImportExcelForm
        {
            [FromForm] public IFormFile File { get; set; } = null!;
        }

        public sealed class UpdateManualForm
        {
            [FromForm] public IFormFile ManualFile { get; set; } = null!;
        }


        // =====================================================================
        // 📌 SPECIFICATIONS PARSING
        // =====================================================================

        private static readonly HashSet<string> KnownFormKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "ManualFile","Images","Certificate",
            "Data.VendorId","Data.CategoryId","Data.ProposedProductCode","Data.ProposedProductName",
            "Data.Description","Data.UnitPrice","Data.EnergyEfficiencyRating","Data.WarrantyMonths",
            "Data.WeightKg","Data.DimensionsCm.Width","Data.DimensionsCm.Height","Data.DimensionsCm.Length",
            "Data.Id"
        };

        private static void FillSpecsFromForm(object dto, IFormCollection form)
        {
            // Case 1: Specifications được gửi dưới dạng JSON nguyên khối
            if (form.TryGetValue("Data.Specifications", out var jsonVals) &&
                !string.IsNullOrWhiteSpace(jsonVals))
            {
                try
                {
                    var jsonString = jsonVals.ToString();
                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString ?? "{}")
                               ?? new Dictionary<string, object>();
                    dto.GetType().GetProperty("Specifications")?.SetValue(dto, dict);
                    return;
                }
                catch { }
            }

            // Case 2: Swagger flatten
            var specs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in form)
            {
                var key = kv.Key;
                if (key.StartsWith("Data.", StringComparison.OrdinalIgnoreCase)) continue;
                if (KnownFormKeys.Contains(key)) continue;

                var raw = kv.Value.ToString();

                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    specs[key] = dec;
                else if (int.TryParse(raw, out var i))
                    specs[key] = i;
                else
                    specs[key] = raw;
            }

            if (specs.Count > 0)
                dto.GetType().GetProperty("Specifications")?.SetValue(dto, specs);
        }
}
