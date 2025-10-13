using BLL.DTO;
using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
using BLL.Services;
using Controller.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/staff")]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : BaseController
    {
        private readonly IStaffService _staffService;
        private readonly IProductService _productService;


        public StaffController(IStaffService staffService, IProductService productService)
        {
            _staffService = staffService;
            _productService = productService;
        }

        /// <summary>Duyệt đăng ký sản phẩm (Approve)</summary>
        /// <param name="id">Id của ProductRegistration</param>
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveProductRegistrationAsync(
            [FromRoute] ulong id,
            CancellationToken cancellationToken)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var result = await _staffService.ApproveProductRegistrationAsync(id, staffId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" }); }
        }

        /// <summary>Từ chối đăng ký sản phẩm (Reject)</summary>
        /// <param name="id">Id của ProductRegistration</param>
        /// <param name="reason">Lý do từ chối</param>
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectProductRegistrationAsync(
            [FromRoute] ulong id,
            [FromQuery] string reason,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var staffId = GetCurrentUserId();
                var result = await _staffService.RejectProductRegistrationAsync(id, staffId, reason, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" }); }
        }

        /// <summary>
        /// Get toàn bộ sản phẩm đã đăng ký của nhà cung cấp (vendor) hiện tại
        /// </summary>
        /// <returns></returns>
        /// <param name="id"></param>
        [HttpGet("vendor/products")]
        [EndpointSummary("Get All Product By VendorID")]
        [EndpointDescription("Lấy toàn bộ thông tin sản phẩm đã đăng ký theo VendorID cho Staff duyệt.")]
        public async Task<ActionResult<APIResponse>> GetAllProductRegistrationsByVendorId([FromQuery] ulong id)
        {
            try
            {
                var result = await _productService.GetAllProductByVendorIdAsync(id, GetCancellationToken());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

        }
    }
}
