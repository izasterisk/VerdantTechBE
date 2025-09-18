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
    public class FarmProfileController : BaseController
    {
        private readonly IFarmProfileService _service;

        public FarmProfileController(IFarmProfileService service)
        {
            _service = service;
        }

        /// <summary>Create a farm profile for current user</summary>
        [HttpPost]
        [EndpointSummary("Create Farm Profile")]
        public async Task<ActionResult<APIResponse>> Create([FromBody] FarmProfileCreateDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.CreateAsync(userId, dto, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Get a farm profile by id (must be owned by current user)</summary>
        [HttpGet("{id}")] // removed :ulong
        [EndpointSummary("Get Farm Profile By Farm Id")]
        public async Task<ActionResult<APIResponse>> GetById([FromRoute] ulong id)
        {
            try
            {
                var result = await _service.GetAsync(id, GetCancellationToken());
                if (result == null) return ErrorResponse("Farm profile not found", HttpStatusCode.NotFound);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Get all farm profiles of current user</summary>
        [HttpGet("User")]
        [EndpointSummary("Get Farm Profiles By User ID")]
        public async Task<ActionResult<APIResponse>> GetAllFarmByUserId()
        {
            try
            {
                var userId = GetCurrentUserId();
                var list = await _service.GetAllByUserIdAsync(userId, GetCancellationToken());
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>Update a farm profile (must be owned by current user)</summary>
        [HttpPut("{id}")]
        [EndpointSummary("Update Farm Profile")]
        public async Task<ActionResult<APIResponse>> Update([FromRoute] ulong id, [FromBody] FarmProfileUpdateDTO dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var userId = GetCurrentUserId();
                var result = await _service.UpdateAsync(id, userId, dto, GetCancellationToken());
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
        // [HttpDelete("{id}")]
        // [EndpointSummary("Delete Farm Profile (Hard)")]
        // public async Task<ActionResult<APIResponse>> Delete([FromRoute] ulong id)
        // {
        //     try
        //     {
        //         var userId = GetCurrentUserId();
        //         var ok = await _service.DeleteAsync(id, userId);
        //         if (!ok) return ErrorResponse("Farm profile not found", HttpStatusCode.NotFound);
        //         return SuccessResponse("Deleted");
        //     }
        //     catch (Exception ex)
        //     {
        //         return HandleException(ex);
        //     }
        // }
    }
}
