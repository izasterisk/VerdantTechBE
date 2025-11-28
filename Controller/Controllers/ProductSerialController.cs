using BLL.DTO.ProductSerial;
using BLL.Interfaces;
using DAL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/product-serials")]
    public class ProductSerialController : ControllerBase
    {
        private readonly IBatchInventoryService _service;

        public ProductSerialController(IBatchInventoryService inventoryService)
        {
            _service = inventoryService;
        }

        // ===========================================
        // GET ALL SERIAL BY PRODUCT
        // ===========================================
        [HttpGet("product/{productId}")]
        [EndpointSummary ( "Get all serials by product ID.")]
        public async Task<IActionResult> GetByProductId(ulong productId, CancellationToken ct = default)
        {
            var result = await _service.GetAllSerialsByProductIdAsync(productId, ct);
            return Ok(result);
        }

        // ===========================================
        // GET ALL SERIAL BY BATCH
        // ===========================================
        [HttpGet("batch/{batchId}")]
        [EndpointSummary("Get all serials by batch ID.")]
        public async Task<IActionResult> GetByBatchId(ulong batchId, CancellationToken ct = default)
        {
            var result = await _service.GetAllSerialsByBatchIdAsync(batchId, ct);
            return Ok(result);
        }

        // ===========================================
        // UPDATE SERIAL STATUS
        // ===========================================
        [HttpPatch("{serialId}/status")]
        [EndpointSummary("Update status of a product serial.")]
        public async Task<IActionResult> UpdateStatus(ulong serialId, [FromBody] ProductSerialStatusUpdateDTO dto, CancellationToken ct = default)
        {
            if (dto.Id != serialId)
                return BadRequest(new { error = "Route ID and DTO ID mismatch." });

            try
            {
                await _service.UpdateSerialStatusAsync(serialId, dto.Status, ct);
                return Ok(new { message = "Serial status updated successfully." });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { error = e.Message });
            }
        }
    }
}
