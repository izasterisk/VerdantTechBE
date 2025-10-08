using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
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

        public StaffController(IStaffService staffService)
        {
            _staffService = staffService;
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
    }
}
