using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.SustainabilityCertifications;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class SustainabilityCertificationsController : BaseController
{
    private readonly ISustainabilityCertificationsService _sustainabilityCertificationsService;
    
    public SustainabilityCertificationsController(ISustainabilityCertificationsService sustainabilityCertificationsService)
    {
        _sustainabilityCertificationsService = sustainabilityCertificationsService;
    }

    /// <summary>
    /// Tạo sustainability certification mới
    /// </summary>
    /// <param name="dto">Thông tin sustainability certification cần tạo</param>
    /// <returns>Thông tin sustainability certification đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Create New Sustainability Certification")]
    [EndpointDescription("Description và IssuingBody không bắt buộc.")]
    public async Task<ActionResult<APIResponse>> CreateSustainabilityCertification([FromBody] SustainabilityCertificationsCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var certification = await _sustainabilityCertificationsService.CreateSustainabilityCertificationAsync(dto);
            return SuccessResponse(certification, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin sustainability certification theo ID
    /// </summary>
    /// <param name="id">ID của sustainability certification</param>
    /// <returns>Thông tin sustainability certification</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Get Sustainability Certification By ID")]
    public async Task<ActionResult<APIResponse>> GetSustainabilityCertificationById(ulong id)
    {
        try
        {
            var certification = await _sustainabilityCertificationsService.GetSustainabilityCertificationByIdAsync(id);
            
            if (certification == null)
                return ErrorResponse($"Không tìm thấy sustainability certification với ID {id}", HttpStatusCode.NotFound);

            return SuccessResponse(certification);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả sustainability certifications với phân trang và filter theo category
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="category">Category để filter (Organic, Environmental, FairTrade, FoodSafety, Social, Energy)</param>
    /// <returns>Danh sách sustainability certifications có phân trang</returns>
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Get All Sustainability Certifications")]
    [EndpointDescription("GET toàn bộ certifications, có thể lọc certifications theo category nếu muốn. Mẫu: /api/SustainabilityCertifications?page=2&pageSize=20&category=Organic")]
    public async Task<ActionResult<APIResponse>> GetAllSustainabilityCertifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? category = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var certifications = await _sustainabilityCertificationsService.GetAllSustainabilityCertificationsAsync(page, pageSize, category);
            return SuccessResponse(certifications);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật thông tin sustainability certification
    /// </summary>
    /// <param name="id">ID của sustainability certification</param>
    /// <param name="dto">Thông tin sustainability certification cần cập nhật</param>
    /// <returns>Thông tin sustainability certification đã cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [EndpointSummary("Update Sustainability Certification")]
    public async Task<ActionResult<APIResponse>> UpdateSustainabilityCertification(ulong id, [FromBody] SustainabilityCertificationsUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var certification = await _sustainabilityCertificationsService.UpdateSustainabilityCertificationAsync(id, dto);
            return SuccessResponse(certification);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả categories
    /// </summary>
    /// <returns>Danh sách categories</returns>
    [HttpGet("categories")]
    [AllowAnonymous]
    [EndpointSummary("Get All Categories")]
    [EndpointDescription("Lấy danh sách tất cả sustainability certification categories")]
    public async Task<ActionResult<APIResponse>> GetAllCategories()
    {
        try
        {
            var categories = await _sustainabilityCertificationsService.GetAllCategoriesAsync();
            return SuccessResponse(categories);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}