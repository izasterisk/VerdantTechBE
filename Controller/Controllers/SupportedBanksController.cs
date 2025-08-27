using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.SupportedBanks;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportedBanksController : ControllerBase
{
    private readonly ISupportedBanksService _supportedBanksService;
    
    public SupportedBanksController(ISupportedBanksService supportedBanksService)
    {
        _supportedBanksService = supportedBanksService;
    }

    /// <summary>
    /// Tạo ngân hàng hỗ trợ mới
    /// </summary>
    /// <param name="dto">Thông tin ngân hàng cần tạo</param>
    /// <returns>Thông tin ngân hàng đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Create New Supported Bank")]
    [EndpointDescription("Không bắt buộc ImageUrl.")]
    public async Task<ActionResult<APIResponse>> CreateSupportedBank([FromBody] SupportedBanksCreateDTO dto)
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

            var supportedBank = await _supportedBanksService.CreateSupportedBankAsync(dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Data = supportedBank;
            
            return CreatedAtAction(nameof(GetSupportedBankById), new { id = supportedBank.Id }, response);
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
    /// Lấy thông tin ngân hàng hỗ trợ theo ID
    /// </summary>
    /// <param name="id">ID của ngân hàng</param>
    /// <returns>Thông tin ngân hàng</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Get Supported Bank By ID")]
    public async Task<ActionResult<APIResponse>> GetSupportedBankById(ulong id)
    {
        var response = new APIResponse();
        
        try
        {
            var supportedBank = await _supportedBanksService.GetSupportedBankByIdAsync(id);
            
            if (supportedBank == null)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add($"Không tìm thấy ngân hàng với ID {id}");
                return NotFound(response);
            }

            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = supportedBank;
            
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
    /// Lấy thông tin ngân hàng hỗ trợ theo mã ngân hàng
    /// </summary>
    /// <param name="code">Mã ngân hàng</param>
    /// <returns>Thông tin ngân hàng</returns>
    [HttpGet("by-code/{code}")]
    [AllowAnonymous]
    [EndpointSummary("Get Supported Bank By Bank Code")]
    public async Task<ActionResult<APIResponse>> GetSupportedBankByBankCode(string code)
    {
        var response = new APIResponse();
        
        try
        {
            var supportedBank = await _supportedBanksService.GetSupportedBankByBankCodeAsync(code);
            
            if (supportedBank == null)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add($"Không tìm thấy ngân hàng với mã {code}");
                return NotFound(response);
            }

            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = supportedBank;
            
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
    /// Lấy danh sách tất cả ngân hàng hỗ trợ với phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách ngân hàng hỗ trợ có phân trang</returns>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Get All Supported Banks")]
    [EndpointDescription("Lấy danh sách tất cả ngân hàng hỗ trợ với phân trang. Mẫu: /api/SupportedBanks?page=2&pageSize=20")]
    public async Task<ActionResult<APIResponse>> GetAllSupportedBanks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

            var supportedBanks = await _supportedBanksService.GetAllSupportedBanksAsync(page, pageSize);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = supportedBanks;
            
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
    /// Cập nhật thông tin ngân hàng hỗ trợ
    /// </summary>
    /// <param name="id">ID của ngân hàng</param>
    /// <param name="dto">Thông tin ngân hàng cần cập nhật</param>
    /// <returns>Thông tin ngân hàng đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Update Supported Bank")]
    public async Task<ActionResult<APIResponse>> UpdateSupportedBank(ulong id, [FromBody] SupportedBanksUpdateDTO dto)
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

            var supportedBank = await _supportedBanksService.UpdateSupportedBankAsync(id, dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = supportedBank;
            
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