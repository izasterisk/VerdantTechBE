using BLL.DTO;
using BLL.DTO.FarmProfile;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FarmProfileController : BaseController
    {
        private readonly IFarmProfileService _farmProfileService;

        public FarmProfileController(IFarmProfileService farmProfileService)
        {
            _farmProfileService = farmProfileService;
        }

        /// <summary>
        /// Tạo hồ sơ trang trại cho người dùng hiện tại
        /// </summary>
        /// <param name="dto">Thông tin hồ sơ trang trại cần tạo</param>
        /// <returns>Thông tin hồ sơ trang trại đã tạo</returns>
        [HttpPost]
        [EndpointSummary("Create Farm Profile")]
        [EndpointDescription("Tạo hồ sơ trang trại mới cho người dùng hiện tại. Id chủ trang trại sẽ được lấy từ token đăng nhập.")]
        public async Task<ActionResult<APIResponse>> CreateFarmProfile([FromBody] FarmProfileCreateDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                var userId = GetCurrentUserId();
                var result = await _farmProfileService.CreateFarmProfileAsync(userId, dto, GetCancellationToken());
                return SuccessResponse(result, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Lấy thông tin hồ sơ trang trại theo ID
        /// </summary>
        /// <param name="id">ID của hồ sơ trang trại</param>
        /// <returns>Thông tin hồ sơ trang trại</returns>
        [HttpGet("{id}")]
        [EndpointSummary("Get Farm Profile By ID")]
        [EndpointDescription("Lấy thông tin hồ sơ trang trại theo ID.")]
        public async Task<ActionResult<APIResponse>> GetFarmProfileById([FromRoute] ulong id)
        {
            try
            {
                var result = await _farmProfileService.GetFarmProfileByFarmIdAsync(id, GetCancellationToken());
                
                if (result == null) 
                    return ErrorResponse($"Không tìm thấy hồ sơ trang trại với ID {id}", HttpStatusCode.NotFound);
                
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả hồ sơ trang trại theo User ID
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách hồ sơ trang trại của người dùng</returns>
        [HttpGet("User/{userId}")]
        [EndpointSummary("Get Farm Profiles By User ID")]
        [EndpointDescription("Lấy danh sách tất cả hồ sơ trang trại theo User ID")]
        public async Task<ActionResult<APIResponse>> GetAllFarmProfilesByUserId([FromRoute] ulong userId)
        {
            try
            {
                var list = await _farmProfileService.GetAllFarmProfileByUserIdAsync(userId, GetCancellationToken());
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ trang trại
        /// </summary>
        /// <param name="id">ID của hồ sơ trang trại</param>
        /// <param name="dto">Thông tin hồ sơ trang trại cần cập nhật</param>
        /// <returns>Thông tin hồ sơ trang trại đã cập nhật</returns>
        [HttpPut("{id}")]
        [EndpointSummary("Update Farm Profile")]
        [EndpointDescription("Cập nhật thông tin hồ sơ trang trại.")]
        public async Task<ActionResult<APIResponse>> UpdateFarmProfile([FromRoute] ulong id, [FromBody] FarmProfileUpdateDTO dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            
            try
            {
                var result = await _farmProfileService.UpdateFarmProfileAsync(id, dto, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return ErrorResponse("Không tìm thấy hồ sơ trang trại", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
