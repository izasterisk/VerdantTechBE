using BLL.DTO;
using BLL.DTO.FarmProfile;
using BLL.Interfaces;
using DAL.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using System.Net;
using System.Security.Claims;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FarmProfilesController : BaseController
    {
        private readonly IFarmProfileService _service;

        public FarmProfilesController(IFarmProfileService service)
        {
            _service = service;
        }

        private bool TryGetCurrentUserId(out ulong userId)
        {
            userId = 0;
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(idStr) && ulong.TryParse(idStr, out userId);
        }

        /// <summary>Create a farm profile for current user</summary>
        [HttpPost]
        [EndpointSummary("Create Farm Profile")]
        public async Task<ActionResult<APIResponse>> Create([FromBody] FarmProfileCreateDto dto, CancellationToken ct)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            if (!TryGetCurrentUserId(out var userId)) return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            try
            {
                var result = await _service.CreateAsync(userId, dto, ct);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Get a farm profile by id (must be owned by current user)</summary>
        [HttpGet("{id}")] // removed :ulong
        [EndpointSummary("Get Farm Profile By Id")]
        public async Task<ActionResult<APIResponse>> GetById([FromRoute] ulong id, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId)) return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            try
            {
                var result = await _service.GetAsync(id, userId, ct);
                if (result == null) return ErrorResponse("Farm profile not found", HttpStatusCode.NotFound);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Get all farm profiles of current user</summary>
        [HttpGet("me")]
        [EndpointSummary("Get My Farm Profiles")]
        public async Task<ActionResult<APIResponse>> GetMine(CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId)) return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            try
            {
                var list = await _service.GetAllByUserIdAsync(userId, ct);
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>(Optional) Admin/Staff: Get all farm profiles of a specific user (require proper policy)</summary>
        [HttpGet] // removed :ulong
        [Authorize(Policy = "CanViewUserProfiles")]
        [EndpointSummary("Get Farm Profiles By UserId")]
        public async Task<ActionResult<APIResponse>> GetAllByUser( CancellationToken ct)
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr)) return ErrorResponse("User ID claim missing", HttpStatusCode.BadRequest);
            ulong userId = ulong.Parse(idStr);

            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                var list = await _service.GetAllByUserIdAsync(userId, ct);
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Update a farm profile (must be owned by current user)</summary>
        [HttpPut("{id}")] // removed :ulong
        [EndpointSummary("Update Farm Profile")]
        public async Task<ActionResult<APIResponse>> Update([FromRoute] ulong id, [FromBody] FarmProfileUpdateDTO dto, CancellationToken ct)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            if (!TryGetCurrentUserId(out var userId)) return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            try
            {
                var result = await _service.UpdateAsync(id, userId, dto, ct);
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return ErrorResponse("Farm profile not found or access denied.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Delete a farm profile (hard delete) owned by current user</summary>
        [HttpDelete("{id}")] // removed :ulong
        [EndpointSummary("Delete Farm Profile (Hard)")]
        public async Task<ActionResult<APIResponse>> Delete([FromRoute] ulong id, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId)) return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            try
            {
                var ok = await _service.DeleteAsync(id, userId, ct);
                if (!ok) return ErrorResponse("Farm profile not found", HttpStatusCode.NotFound);
                return SuccessResponse("Deleted");
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
