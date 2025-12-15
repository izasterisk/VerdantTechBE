using System.IO;
using BLL.DTO.BatchInventory;
using BLL.Interfaces;
using BLL.Services;
using BLL.Helpers.Excel;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchInventoryController : ControllerBase
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
        public async Task<IActionResult> GetAll( int page = 1, int pageSize = 20,CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(page, pageSize, ct);
            return Ok(result);
        }

        // ============================
        // GET BY PRODUCT ID
        // ============================

        [HttpGet("product/{productId}")]
        [EndpointSummary("Get batch inventories by product ID.")]
        [EndpointDescription("Returns a paginated list of batch inventories linked to a specific product.")]
        public async Task<IActionResult> GetByProduct(ulong productId, int page = 1, int pageSize = 20,CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByProductIdAsync(productId, page, pageSize, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            
        }

        // ============================
        // GET BY VENDOR ID
        // ============================

        [HttpGet("vendor/{vendorId}")]
        [EndpointSummary("Get batch inventories by vendor ID.")]
        [EndpointDescription("Returns a paginated list of batch inventories associated with a specific vendor.")]
        public async Task<IActionResult> GetByVendor( ulong vendorId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByVendorIdAsync(vendorId, page, pageSize, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ============================
        // GET BY ID
        // ============================

        [HttpGet("{id}")]
        [EndpointSummary("Get batch inventory by ID.")]
        [EndpointDescription("Returns a single batch inventory entry by its unique identifier.")]
        public async Task<IActionResult> GetById(ulong id, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetByIdAsync(id, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ============================
        // CREATE
        // ============================

        [HttpPost]
        [EndpointSummary("Create a new batch inventory entry.")]
        [EndpointDescription("Creates a new batch inventory record using the provided information.")]
        public async Task<IActionResult> Create([FromBody] BatchInventoryCreateDTO dto, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.CreateAsync(dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ============================
        // UPDATE
        // ============================

        [HttpPut("{id}")]
        [EndpointSummary("Update an existing batch inventory.")]
        [EndpointDescription("Updates the details of an existing batch inventory record using the provided DTO.")]
        public async Task<IActionResult> Update(ulong id, [FromBody] BatchInventoryUpdateDTO dto, CancellationToken ct = default)
        {
            dto.Id = id;

            try
            {
                var result = await _service.UpdateAsync(dto, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
          
        }

        // ============================
        // DELETE
        // ============================

        [HttpDelete("{id}")]
        [EndpointSummary("Delete a batch inventory entry.")]
        [EndpointDescription("Delete a batch inventory record by its unique identifier.")]
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

        


        // ===========================================
        // GET SERIALS BY BATCH ID
        // ===========================================
        [HttpGet("{batchId}/serials")]
        [EndpointSummary ("Get all serials of a batch.")]
        public async Task<IActionResult> GetSerialsByBatch(ulong batchId, CancellationToken ct = default)
        {
            var serials = await _service.GetAllSerialsByBatchIdAsync(batchId, ct);
            return Ok(serials);
        }

        // ===========================================
        // EXCEL IMPORT
        // ===========================================
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Import BatchInventories từ file Excel")]
        [EndpointDescription("Import nhiều BatchInventory từ file Excel. " +
            "File Excel phải có các cột: ProductId, BatchNumber, LotNumber, Quantity, UnitCostPrice " +
            "và các trường tùy chọn khác. SKU sẽ được tự động tạo.")]
        public async Task<ActionResult<BatchInventoryImportResponseDTO>> ImportFromExcel(
            [FromForm] IFormFile file,
            CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File Excel không được để trống.");

            if (!ExcelHelper.ValidateExcelFormat(file.FileName))
                return BadRequest("File phải có định dạng .xlsx hoặc .xls");

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _importService.ImportFromExcelAsync(stream, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi xử lý file Excel: {ex.Message}" });
            }
        }

        [HttpGet("import/template")]
        [EndpointSummary("Tải template Excel cho BatchInventory import")]
        [EndpointDescription("Tải file Excel template với các cột mẫu để import BatchInventory.")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                
                var worksheet = package.Workbook.Worksheets.Add("BatchInventories");

                // Header row
                worksheet.Cells[1, 1].Value = "ProductId";
                worksheet.Cells[1, 2].Value = "VendorId";
                worksheet.Cells[1, 3].Value = "BatchNumber";
                worksheet.Cells[1, 4].Value = "LotNumber";
                worksheet.Cells[1, 5].Value = "Quantity";
                worksheet.Cells[1, 6].Value = "UnitCostPrice";
                worksheet.Cells[1, 7].Value = "ExpiryDate";
                worksheet.Cells[1, 8].Value = "ManufacturingDate";
                worksheet.Cells[1, 9].Value = "SerialNumbers";

                // Example row
                worksheet.Cells[2, 1].Value = 1;
                worksheet.Cells[2, 2].Value = 1;
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
                    "BatchInventory_Import_Template.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi tạo template: {ex.Message}" });
            }
        }
    }
}
