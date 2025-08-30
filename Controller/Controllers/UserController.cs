using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.User;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Tạo người dùng mới
    /// </summary>
    /// <param name="dto">Thông tin người dùng cần tạo</param>
    /// <returns>Thông tin người dùng đã tạo</returns>
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Create New User (note.)")]
    [EndpointDescription("Nếu không truyền role thì mặc định sẽ là customer, muốn tạo role khác thì truyền Role tương ứng. " +
                         "Nếu role=admin/manager thì sẽ được tự động IsVerified=true, " +
                         "không gửi Verification Email nhưng thay vào đó sẽ gửi email tài khoản được cấp.")]
    public async Task<ActionResult<APIResponse>> CreateUser([FromBody] UserCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return SuccessResponse(user, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin người dùng theo ID
    /// </summary>
    /// <param name="id">ID của người dùng</param>
    /// <returns>Thông tin người dùng</returns>
    [HttpGet("{id}")]
    [Authorize]
    [EndpointSummary("Get User By ID")]
    public async Task<ActionResult<APIResponse>> GetUserById(ulong id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
                return ErrorResponse($"Không tìm thấy người dùng với ID {id}", HttpStatusCode.NotFound);

            return SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng với phân trang và filter theo role
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="role">Role để filter (Customer, Seller, Admin, Manager). Mặc định: Customer</param>
    /// <returns>Danh sách người dùng có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Get All Users (note.)")]
    [EndpointDescription("Lọc người dùng theo role. Nếu không ghi ra, chỉ trả về Customer. Mẫu: /api/User?page=2&pageSize=20&role=admin")]
    public async Task<ActionResult<APIResponse>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var users = await _userService.GetAllUsersAsync(page, pageSize, role);
            return SuccessResponse(users);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật thông tin người dùng
    /// </summary>
    /// <param name="id">ID của người dùng</param>
    /// <param name="dto">Thông tin người dùng cần cập nhật</param>
    /// <returns>Thông tin người dùng đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize]
    [EndpointSummary("Update User")]
    public async Task<ActionResult<APIResponse>> UpdateUser(ulong id, [FromBody] UserUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            return SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Thay đổi trạng thái người dùng
    /// </summary>
    /// <param name="id">ID của người dùng</param>
    /// <param name="status">Trạng thái mới (Active, Inactive, Suspended, Deleted)</param>
    /// <returns>Thông tin người dùng đã cập nhật trạng thái</returns>
    [HttpPatch("{id}/status")]
    [Authorize]
    [EndpointSummary("Change User Status (note.)")]
    [EndpointDescription("Thay đổi trạng thái của người dùng thành 1 trong: Active, Inactive, Suspended, Deleted. " +
                         "Active = bình thường, Inactive = tạm dừng (người dùng có thể tự đặt), " +
                         "Suspended = đình chỉ (bị admin/manager chém), Deleted = xóa (chỉ là bị soft delete, không xóa hoàn toàn.")]
    public async Task<ActionResult<APIResponse>> ChangeUserStatus(ulong id, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ErrorResponse("Status parameter is required");
        }

        try
        {
            var user = await _userService.ChangeUserStatusAsync(id, status);
            return SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}