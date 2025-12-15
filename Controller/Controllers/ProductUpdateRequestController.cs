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
}
