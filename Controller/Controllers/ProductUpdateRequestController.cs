using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.ProductUpdateRequest;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Globalization;

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
    [EndpointDescription("Tạo yêu cầu cập nhật sản phẩm mới, bao gồm upload ảnh và file manual.")]
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
                KnownFormKeys
            );

            // TODO: Get userId from authenticated user
            ulong userId = 1; // Placeholder
            
            var result = await _service.CreateProductUpdateRequestAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    private static readonly HashSet<string> KnownFormKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CategoryId", "ProductCode", "ProductName", "Description",
        "UnitPrice", "DiscountPercentage", "EnergyEfficiencyRating",
        "ManualFile", "WarrantyMonths", "WeightKg", "Images"
    };
}
