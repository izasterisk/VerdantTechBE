using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.SupportedBanks;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class SupportedBanksController : BaseController
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
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Create New Supported Bank")]
    [EndpointDescription("Không bắt buộc ImageUrl.")]
    public async Task<ActionResult<APIResponse>> CreateSupportedBank([FromBody] SupportedBanksCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var supportedBank = await _supportedBanksService.CreateSupportedBankAsync(dto);
            return SuccessResponse(supportedBank, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
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
        try
        {
            var supportedBank = await _supportedBanksService.GetSupportedBankByIdAsync(id);
            
            if (supportedBank == null)
                return ErrorResponse($"Không tìm thấy ngân hàng với ID {id}", HttpStatusCode.NotFound);

            return SuccessResponse(supportedBank);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
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
        try
        {
            var supportedBank = await _supportedBanksService.GetSupportedBankByBankCodeAsync(code);
            
            if (supportedBank == null)
                return ErrorResponse($"Không tìm thấy ngân hàng với mã {code}", HttpStatusCode.NotFound);

            return SuccessResponse(supportedBank);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
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
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var supportedBanks = await _supportedBanksService.GetAllSupportedBanksAsync(page, pageSize);
            return SuccessResponse(supportedBanks);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật thông tin ngân hàng hỗ trợ
    /// </summary>
    /// <param name="id">ID của ngân hàng</param>
    /// <param name="dto">Thông tin ngân hàng cần cập nhật</param>
    /// <returns>Thông tin ngân hàng đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Update Supported Bank")]
    public async Task<ActionResult<APIResponse>> UpdateSupportedBank(ulong id, [FromBody] SupportedBanksUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var supportedBank = await _supportedBanksService.UpdateSupportedBankAsync(id, dto);
            return SuccessResponse(supportedBank);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}