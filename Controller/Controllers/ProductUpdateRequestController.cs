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
    /// Xóa yêu cầu cập nhật sản phẩm
    /// </summary>
    /// <param name="requestId">ID của yêu cầu cập nhật</param>
    /// <returns>Thông báo xóa thành công</returns>
    [HttpDelete("{requestId}")]
    [Authorize]
    [EndpointSummary("Delete Product Update Request")]
    [EndpointDescription("Xóa yêu cầu cập nhật sản phẩm. Chỉ có thể xóa các yêu cầu đang chờ xử lý (Pending).")]
    public async Task<ActionResult<APIResponse>> DeleteProductUpdateRequest(ulong requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.DeleteProductUpdateRequestAsync(userId, requestId, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả yêu cầu cập nhật sản phẩm
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10, tối đa: 100)</param>
    /// <param name="status">Trạng thái để filter (Pending, Approved, Rejected). Mặc định: tất cả</param>
    /// <param name="vendorId">ID của vendor để filter theo vendor. Mặc định: tất cả</param>
    /// <returns>Danh sách yêu cầu cập nhật sản phẩm có phân trang</returns>
    [HttpGet]
    [Authorize]
    [EndpointSummary("Get All Product Update Requests")]
    [EndpointDescription("Lọc yêu cầu cập nhật sản phẩm theo trạng thái: Pending, Approved, Rejected và/hoặc theo vendorId. Nếu không ghi status hoặc vendorId, trả về tất cả. Mẫu: /api/ProductUpdateRequest?page=1&pageSize=20&status=Pending&vendorId=123")]
    public async Task<ActionResult<APIResponse>> GetAllProductUpdateRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] ulong? vendorId = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            // Parse status if provided
            ProductRegistrationStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ProductRegistrationStatus>(status, true, out var parsedStatus))
                {
                    statusEnum = parsedStatus;
                }
                else
                {
                    return ErrorResponse($"Invalid status value. Valid values are: {string.Join(", ", Enum.GetNames(typeof(ProductRegistrationStatus)))}");
                }
            }

            var requests = await _service.GetAllProductUpdateRequestsAsync(page, pageSize, statusEnum, vendorId, GetCancellationToken());
            return SuccessResponse(requests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy lịch sử các thay đổi của sản phẩm
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10, tối đa: 100)</param>
    /// <returns>Danh sách lịch sử thay đổi sản phẩm có phân trang</returns>
    [HttpGet("product/{productId}/history")]
    [Authorize]
    [EndpointSummary("Get Product History")]
    [EndpointDescription("Lấy toàn bộ lịch sử các thay đổi đã được duyệt của một sản phẩm. Mẫu: /api/ProductUpdateRequest/product/1/history?page=1&pageSize=10")]
    public async Task<ActionResult<APIResponse>> GetAllProductHistories(ulong productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var histories = await _service.GetAllProductHistoriesAsync(productId, page, pageSize, GetCancellationToken());
            return SuccessResponse(histories);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
