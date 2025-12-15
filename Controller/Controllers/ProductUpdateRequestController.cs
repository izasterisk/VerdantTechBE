using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.ProductUpdateRequest;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Globalization;
using DAL.Data;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class ProductUpdateRequestController : BaseController
{
    private readonly IProductUpdateRequestService _service;
    
    public ProductUpdateRequestController(IProductUpdateRequestService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo yêu cầu cập nhật sản phẩm
    /// </summary>
    /// <param name="dto">Thông tin yêu cầu cập nhật</param>
    /// <returns>Thông tin yêu cầu đã tạo</returns>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Create Product Update Request")]
    [EndpointDescription("Update gì thì truyền cái đấy, không update cái gì thì cứ để null. " +
                         "Thêm ảnh mới vào thì dùng ImagesToAdd, xóa ảnh thì dùng ImagesToDelete.")]
    public async Task<ActionResult<APIResponse>> CreateProductUpdateRequest([FromForm] ProductUpdateRequestCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            // Parse Specifications và DimensionsCm từ form-data
            ParseDictionaryFields(
                Request.Form,
                specs => dto.Specifications = specs,
                dims => dto.DimensionsCm = dims,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "ProductId", "ProductCode", "ProductName", "Description",
                    "UnitPrice", "DiscountPercentage", "EnergyEfficiencyRating",
                    "ManualFile", "WarrantyMonths", "WeightKg", "ImagesToAdd", "ImagesToDelete"
                }
            );

            var userId = GetCurrentUserId();
            var result = await _service.CreateProductUpdateRequestAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật sản phẩm (Approve/Reject)
    /// </summary>
    /// <param name="requestId">ID của yêu cầu cập nhật</param>
    /// <param name="dto">Thông tin xử lý (Status và RejectionReason)</param>
    /// <returns>Thông tin yêu cầu đã xử lý</returns>
    [HttpPatch("{requestId}")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Process Product Update Request")]
    [EndpointDescription("Duyệt hoặc từ chối yêu cầu cập nhật sản phẩm. Status có thể là Approved hoặc Rejected. " +
                         "Nếu Rejected thì có thể cung cấp RejectionReason.")]
    public async Task<ActionResult<APIResponse>> ProcessProductUpdateRequest(ulong requestId, [FromBody] ProductUpdateRequestUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var result = await _service.ProcessProductUpdateRequestAsync(staffId, requestId, dto, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả yêu cầu cập nhật sản phẩm với phân trang và filter theo status
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="status">Status để filter (Pending, Approved, Rejected). Để trống để lấy tất cả</param>
    /// <returns>Danh sách yêu cầu cập nhật sản phẩm có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get All Product Update Requests (note.)")]
    [EndpointDescription("Lọc yêu cầu theo status. Nếu không truyền status thì trả về tất cả. " +
                         "Mẫu: /api/ProductUpdateRequest?page=1&pageSize=10&status=Pending")]
    public async Task<ActionResult<APIResponse>> GetAllProductUpdateRequests(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] ProductRegistrationStatus? status = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var result = await _service.GetAllProductUpdateRequestsAsync(page, pageSize, status, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
