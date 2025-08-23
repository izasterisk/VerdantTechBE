using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.User;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
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
    [EndpointSummary("Create New User")]
    [EndpointDescription("Nếu truyền Role = null thì mặc định sẽ là customer, muốn tạo role khác thì truyền Role tương ứng.")]
    public async Task<ActionResult<APIResponse>> CreateUser([FromBody] UserCreateDTO dto)
    {
        var response = new APIResponse();
        
        try
        {
            if (!ModelState.IsValid)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await _userService.CreateUserAsync(dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Data = user;
            
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors.Add(ex.Message);
            return BadRequest(response);
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
    [EndpointDescription("Retrieves user information by their unique identifier")]
    public async Task<ActionResult<APIResponse>> GetUserById(ulong id)
    {
        var response = new APIResponse();
        
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add($"Không tìm thấy người dùng với ID {id}");
                return NotFound(response);
            }

            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = user;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Errors.Add(ex.Message);
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng với phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách người dùng có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Get All Users")]
    [EndpointDescription("Retrieves a paginated list of all users (Admin/Manager access required)")]
    public async Task<ActionResult<APIResponse>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = new APIResponse();
        
        try
        {
            // Validate pagination parameters
            if (page < 1)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Page number must be greater than 0");
                return BadRequest(response);
            }

            if (pageSize < 1 || pageSize > 100)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Page size must be between 1 and 100");
                return BadRequest(response);
            }

            var users = await _userService.GetAllUsersAsync(page, pageSize);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = users;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Errors.Add(ex.Message);
            return StatusCode(500, response);
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
    [EndpointDescription("Updates user information with the provided data")]
    public async Task<ActionResult<APIResponse>> UpdateUser(ulong id, [FromBody] UserUpdateDTO dto)
    {
        var response = new APIResponse();
        
        try
        {
            if (!ModelState.IsValid)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await _userService.UpdateUserAsync(id, dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = user;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors.Add(ex.Message);
            return BadRequest(response);
        }
    }
}