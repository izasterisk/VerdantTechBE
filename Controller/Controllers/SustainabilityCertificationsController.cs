using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.SustainabilityCertifications;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SustainabilityCertificationsController : ControllerBase
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

            var certification = await _sustainabilityCertificationsService.CreateSustainabilityCertificationAsync(dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.Created;
            response.Data = certification;
            
            return CreatedAtAction(nameof(GetSustainabilityCertificationById), new { id = certification.Id }, response);
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
    /// Lấy thông tin sustainability certification theo ID
    /// </summary>
    /// <param name="id">ID của sustainability certification</param>
    /// <returns>Thông tin sustainability certification</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Get Sustainability Certification By ID")]
    public async Task<ActionResult<APIResponse>> GetSustainabilityCertificationById(ulong id)
    {
        var response = new APIResponse();
        
        try
        {
            var certification = await _sustainabilityCertificationsService.GetSustainabilityCertificationByIdAsync(id);
            
            if (certification == null)
            {
                response.Status = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add($"Không tìm thấy sustainability certification với ID {id}");
                return NotFound(response);
            }

            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = certification;
            
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

            var certifications = await _sustainabilityCertificationsService.GetAllSustainabilityCertificationsAsync(page, pageSize, category);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = certifications;
            
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

            var certification = await _sustainabilityCertificationsService.UpdateSustainabilityCertificationAsync(id, dto);
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = certification;
            
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
        var response = new APIResponse();
        
        try
        {
            var categories = await _sustainabilityCertificationsService.GetAllCategoriesAsync();
            
            response.Status = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Data = categories;
            
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
}