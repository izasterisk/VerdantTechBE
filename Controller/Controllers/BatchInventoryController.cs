using System.IO;
using BLL.DTO;
using BLL.DTO.BatchInventory;
using BLL.Interfaces;
using BLL.Services;
using BLL.Helpers.Excel;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Controller.Controllers;
using System.Net;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BatchInventoryController : BaseController
    {
        private readonly IBatchInventoryService _service;
        private readonly BatchInventoryImportService _importService;

        public BatchInventoryController(
            IBatchInventoryService service,
            BatchInventoryImportService importService)
        {
            _service = service;
            _importService = importService;
        }

        // ============================
        // GET ALL (PAGING)
        // ============================

        [HttpGet]
        [EndpointSummary("Get all batch inventories with pagination.")]
        [EndpointDescription("Returns all batch inventories with paging options including page number and page size.")]
        public async Task<ActionResult<APIResponse>> GetAll(int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetAllAsync(page, pageSize, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // GET BY PRODUCT ID
        // ============================

        [HttpGet("product/{productId}")]
        [EndpointSummary("Get batch inventories by product ID.")]
        [EndpointDescription("Returns a paginated list of batch inventories linked to a specific product.")]
        public async Task<ActionResult<APIResponse>> GetByProduct(ulong productId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByProductIdAsync(productId, page, pageSize, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // GET BY VENDOR ID
        // ============================

        [HttpGet("vendor/{vendorId}")]
        [EndpointSummary("Get batch inventories by vendor ID.")]
        [EndpointDescription("Returns a paginated list of batch inventories associated with a specific vendor.")]
        public async Task<ActionResult<APIResponse>> GetByVendor(ulong vendorId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByVendorIdAsync(vendorId, page, pageSize, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // GET BY ID
        // ============================

        [HttpGet("{id}")]
        [EndpointSummary("Get batch inventory by ID.")]
        [EndpointDescription("Returns a single batch inventory entry by its unique identifier.")]
        public async Task<ActionResult<APIResponse>> GetById(ulong id, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                if (result == null)
                    return ErrorResponse("Không tìm thấy batch inventory với ID này.", HttpStatusCode.NotFound);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // CREATE
        // ============================

        [HttpPost]
        [EndpointSummary("Create a new batch inventory entry.")]
        [EndpointDescription("Creates a new batch inventory record using the provided information. VendorId sẽ được tự động lấy từ user đăng nhập.")]
        public async Task<ActionResult<APIResponse>> Create([FromBody] BatchInventoryCreateDTO dto, CancellationToken ct = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                // Tự động lấy VendorId từ user đăng nhập
                var userId = GetCurrentUserId();
                dto.VendorId = userId;
                
                var result = await _service.CreateAsync(dto, ct);
                return SuccessResponse(result, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // UPDATE
        // ============================

        [HttpPut("{id}")]
        [EndpointSummary("Update an existing batch inventory.")]
        [EndpointDescription("Updates the details of an existing batch inventory record using the provided DTO.")]
        public async Task<ActionResult<APIResponse>> Update(ulong id, [FromBody] BatchInventoryUpdateDTO dto, CancellationToken ct = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            dto.Id = id;

            try
            {
                var result = await _service.UpdateAsync(dto, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ============================
        // DELETE
        // ============================

        [HttpDelete("{id}")]
        [EndpointSummary("Delete a batch inventory entry.")]
        [EndpointDescription("Delete a batch inventory record by its unique identifier.")]
        public async Task<ActionResult<APIResponse>> Delete(ulong id, CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return SuccessResponse(null, HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        


        // ===========================================
        // GET SERIALS BY BATCH ID
        // ===========================================
        [HttpGet("{batchId}/serials")]
        [EndpointSummary("Get all serials of a batch.")]
        [EndpointDescription("Lấy danh sách tất cả serial numbers của một batch inventory.")]
        public async Task<ActionResult<APIResponse>> GetSerialsByBatch(ulong batchId, CancellationToken ct = default)
        {
            try
            {
                var serials = await _service.GetAllSerialsByBatchIdAsync(batchId, ct);
                return SuccessResponse(serials);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // ===========================================
        // EXCEL IMPORT
        // ===========================================
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Import BatchInventories từ file Excel")]
        [EndpointDescription("Import nhiều BatchInventory từ file Excel. " +
            "File Excel phải có các cột: ProductId, BatchNumber, LotNumber, Quantity, UnitCostPrice " +
            "và các trường tùy chọn khác. SKU và VendorId sẽ được tự động tạo (VendorId lấy từ user đăng nhập).")]
        public async Task<ActionResult<APIResponse>> ImportFromExcel(
            [FromForm] ImportExcelForm form,
            CancellationToken ct = default)
        {
            if (form.File == null || form.File.Length == 0)
                return ErrorResponse("File Excel không được để trống.", HttpStatusCode.BadRequest);

            if (!ExcelHelper.ValidateExcelFormat(form.File.FileName))
                return ErrorResponse("File phải có định dạng .xlsx hoặc .xls", HttpStatusCode.BadRequest);

            try
            {
                
                // Sử dụng OpenReadStream() và đảm bảo dispose đúng cách
                byte[] fileBytes;
                using (var fileStream = form.File.OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream, ct);
                    fileBytes = memoryStream.ToArray();
                }
                
                using var stream = new MemoryStream(fileBytes);
                var result = await _importService.ImportFromExcelAsync(stream, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("import/template")]
        [EndpointSummary("Tải template Excel cho BatchInventory import")]
        [EndpointDescription("Tải file Excel template với các cột mẫu để import BatchInventory.")]
        public ActionResult DownloadTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                
                var worksheet = package.Workbook.Worksheets.Add("BatchInventories");

                // Header row (VendorId sẽ được tự động lấy từ user đăng nhập)
                worksheet.Cells[1, 1].Value = "ProductId";
                worksheet.Cells[1, 2].Value = "BatchNumber";
                worksheet.Cells[1, 3].Value = "LotNumber";
                worksheet.Cells[1, 4].Value = "Quantity";
                worksheet.Cells[1, 5].Value = "UnitCostPrice";
                worksheet.Cells[1, 6].Value = "ExpiryDate";
                worksheet.Cells[1, 7].Value = "ManufacturingDate";
                worksheet.Cells[1, 8].Value = "SerialNumbers";

                // Example row
                worksheet.Cells[2, 1].Value = 1;
                worksheet.Cells[2, 2].Value = "BATCH001";
                worksheet.Cells[2, 3].Value = "LOT001";
                worksheet.Cells[2, 4].Value = 10;
                worksheet.Cells[2, 5].Value = 90000;
                worksheet.Cells[2, 6].Value = "2025-12-31";
                worksheet.Cells[2, 7].Value = "2025-01-01";
                worksheet.Cells[2, 8].Value = "SN001,SN002,SN003";

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "BatchInventory_Import_Template.xlsx");
            }
            catch (Exception ex)
            {
                // Trả về JSON error response thay vì file
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    APIResponse.Error($"Lỗi khi tạo template Excel: {ex.Message}", HttpStatusCode.InternalServerError));
            }
        }

        // ===========================================
        // EXCEL IMPORT BY CODE/NAME (NEW API)
        // ===========================================
        [HttpPost("import-by-code")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Import BatchInventories từ file Excel bằng ProductCode/ProductName")]
        [EndpointDescription("Import nhiều BatchInventory từ file Excel. " +
            "File Excel phải có các cột: ProductCode (hoặc ProductName), BatchNumber, LotNumber, Quantity, UnitCostPrice " +
            "và các trường tùy chọn khác. ProductId sẽ được tự động tìm từ ProductCode/ProductName. " +
            "SKU và VendorId sẽ được tự động tạo (VendorId lấy từ product).")]
        public async Task<ActionResult<APIResponse>> ImportFromExcelByCode(
            [FromForm] ImportExcelForm form,
            CancellationToken ct = default)
        {
            if (form.File == null || form.File.Length == 0)
                return ErrorResponse("File Excel không được để trống.", HttpStatusCode.BadRequest);

            if (!ExcelHelper.ValidateExcelFormat(form.File.FileName))
                return ErrorResponse("File phải có định dạng .xlsx hoặc .xls", HttpStatusCode.BadRequest);

            try
            {
                // Sử dụng OpenReadStream() và đảm bảo dispose đúng cách
                byte[] fileBytes;
                using (var fileStream = form.File.OpenReadStream())
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream, ct);
                    fileBytes = memoryStream.ToArray();
                }
                
                using var stream = new MemoryStream(fileBytes);
                var result = await _importService.ImportFromExcelByCodeAsync(stream, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("import-by-code/template")]
        [EndpointSummary("Tải template Excel cho BatchInventory import bằng ProductCode/ProductName")]
        [EndpointDescription("Tải file Excel template với các cột mẫu để import BatchInventory. " +
            "Template này sử dụng ProductCode hoặc ProductName thay vì ProductId.")]
        public ActionResult DownloadTemplateByCode()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                
                var worksheet = package.Workbook.Worksheets.Add("BatchInventories");

                // Header row - ProductCode hoặc ProductName thay vì ProductId
                worksheet.Cells[1, 1].Value = "ProductCode";
                worksheet.Cells[1, 2].Value = "ProductName";
                worksheet.Cells[1, 3].Value = "BatchNumber";
                worksheet.Cells[1, 4].Value = "LotNumber";
                worksheet.Cells[1, 5].Value = "Quantity";
                worksheet.Cells[1, 6].Value = "UnitCostPrice";
                worksheet.Cells[1, 7].Value = "ExpiryDate";
                worksheet.Cells[1, 8].Value = "ManufacturingDate";
                worksheet.Cells[1, 9].Value = "SerialNumbers";

                // Example row - sử dụng ProductCode hoặc ProductName
                worksheet.Cells[2, 1].Value = "PROD-001";
                worksheet.Cells[2, 2].Value = "Máy phun thuốc tự động";
                worksheet.Cells[2, 3].Value = "BATCH001";
                worksheet.Cells[2, 4].Value = "LOT001";
                worksheet.Cells[2, 5].Value = 10;
                worksheet.Cells[2, 6].Value = 90000;
                worksheet.Cells[2, 7].Value = "2025-12-31";
                worksheet.Cells[2, 8].Value = "2025-01-01";
                worksheet.Cells[2, 9].Value = "SN001,SN002,SN003";

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "BatchInventory_Import_ByCode_Template.xlsx");
            }
            catch (Exception ex)
            {
                // Trả về JSON error response thay vì file
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    APIResponse.Error($"Lỗi khi tạo template Excel: {ex.Message}", HttpStatusCode.InternalServerError));
            }
        }

        // =====================================================================
        // 📌 FORM MODELS
        // =====================================================================

        public sealed class ImportExcelForm
        {
            [FromForm] public IFormFile File { get; set; } = null!;
        }
    }
}
