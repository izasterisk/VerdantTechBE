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
                         "Nếu role=admin/staff thì sẽ được tự động IsVerified=true, " +
                         "không gửi Verification Email nhưng thay vào đó sẽ gửi email tài khoản được cấp.")]
    public async Task<ActionResult<APIResponse>> CreateUser([FromBody] UserCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var user = await _userService.CreateUserAsync(dto, GetCancellationToken());
            return SuccessResponse(user, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Tạo tài khoản nhân viên mới
    /// </summary>
    /// <param name="dto">Thông tin nhân viên cần tạo</param>
    /// <returns>Thông tin nhân viên đã tạo</returns>
    [HttpPost("staff")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Create New Staff Account")]
    [EndpointDescription("Tạo tài khoản nhân viên mới với mật khẩu tự động được tạo và gửi qua email. " +
                         "Chỉ Admin mới có quyền thực hiện. Nhân viên sẽ nhận được email chứa thông tin đăng nhập.")]
    public async Task<ActionResult<APIResponse>> CreateStaff([FromBody] StaffCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staff = await _userService.CreateStaffAsync(dto, GetCancellationToken());
            return SuccessResponse(staff, HttpStatusCode.Created);
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
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get User By ID")]
    [EndpointDescription("Chỉ Admin/Staff mới có quyền.")]
    public async Task<ActionResult<APIResponse>> GetUserById(ulong id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, GetCancellationToken());
            
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
    /// <param name="role">Role để filter (Customer, Staff, Admin, Vendor). Mặc định: Customer</param>
    /// <returns>Danh sách người dùng có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
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

            var users = await _userService.GetAllUsersAsync(page, pageSize, role, GetCancellationToken());
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
    [EndpointDescription("Cập nhật thông tin người dùng bao gồm cả status. Status có thể là: Active, Inactive, Suspended, Deleted")]
    public async Task<ActionResult<APIResponse>> UpdateUser(ulong id, [FromBody] UserUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto, GetCancellationToken());
            return SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Tạo địa chỉ mới cho người dùng
    /// </summary>
    /// <param name="userId">ID của người dùng</param>
    /// <param name="dto">Thông tin địa chỉ cần tạo</param>
    /// <returns>Thông tin người dùng đã cập nhật với địa chỉ mới</returns>
    [HttpPost("{userId}/address")]
    [Authorize]
    [EndpointSummary("Create User Address")]
    [EndpointDescription("Tạo địa chỉ mới cho người dùng theo ID")]
    public async Task<ActionResult<APIResponse>> CreateUserAddress(ulong userId, [FromBody] UserAddressCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var user = await _userService.CreateUserAddressAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(user, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật địa chỉ người dùng theo ID địa chỉ
    /// </summary>
    /// <param name="addressId">ID của địa chỉ</param>
    /// <param name="dto">Thông tin địa chỉ cần cập nhật</param>
    /// <returns>Thông tin người dùng đã cập nhật với địa chỉ mới</returns>
    [HttpPut("address/{addressId}")]
    [Authorize]
    [EndpointSummary("Update User Address")]
    [EndpointDescription("Cập nhật địa chỉ người dùng theo ID địa chỉ")]
    public async Task<ActionResult<APIResponse>> UpdateUserAddressByAddressId(ulong addressId, [FromBody] UserAddressUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var user = await _userService.UpdateUserAddressByAddressIdAsync(addressId, dto, GetCancellationToken());
            return SuccessResponse(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}