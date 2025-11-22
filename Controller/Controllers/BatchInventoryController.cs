using BLL.DTO.BatchInventory;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchInventoryController : ControllerBase
    {
        private readonly IBatchInventoryService _service;

        public BatchInventoryController(IBatchInventoryService service)
        {
            _service = service;
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
            var result = await _service.GetByProductIdAsync(productId, page, pageSize, ct);
            return Ok(result);
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
                await _service.UpdateAsync(dto, ct);
                return NoContent();
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
        [EndpointDescription("Deletes a batch inventory record by its unique identifier.")]
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

        // ============================
        // QUALITY CHECK
        // ============================

        [HttpPost("{id}/quality-check")]
        [EndpointSummary("Run quality check on a batch inventory.")]
        [EndpointDescription("Updates the quality check status, inspector ID, and notes for a specific batch inventory.")]
        public async Task<IActionResult> QualityCheck( ulong id, [FromBody] BatchInventoryQualityCheckDTO dto, CancellationToken ct = default)
        {
            try
            {
                await _service.QualityCheckAsync(id, dto, ct);
                return Ok(new { message = "Quality check updated successfully" }); ;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
                     
        }
    }
}
