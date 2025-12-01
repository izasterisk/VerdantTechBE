using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.ExportInventory;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class ExportInventoryController : BaseController
{
    private readonly IExportInventoryService _exportInventoryService;
    
    public ExportInventoryController(IExportInventoryService exportInventoryService)
    {
        _exportInventoryService = exportInventoryService;
    }

    /// <summary>
    /// Tạo đơn xuất kho mới (không bao gồm Sale - Sale chỉ dùng khi bán hàng)
    /// </summary>
    /// <param name="dtos">Danh sách thông tin đơn xuất kho cần tạo</param>
    /// <returns>Danh sách đơn xuất kho đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Create Export Inventories")]
    [EndpointDescription("Tạo đơn xuất kho cho các loại: ReturnToVendor, Damage, Loss, Adjustment. " +
                         "không được nhập MovementType = Sale vì chỉ được sử dụng khi xuất hàng bán qua OrderService. " +
                         "Chỉ Admin/Staff mới có quyền thực hiện.")]
    public async Task<ActionResult<APIResponse>> CreateExportInventories([FromBody] List<ExportInventoryCreateDTO> dtos)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var exportInventories = await _exportInventoryService.CreateExportInventoriesAsync(staffId, dtos, GetCancellationToken());
            return SuccessResponse(exportInventories, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin đơn xuất kho theo ID
    /// </summary>
    /// <param name="id">ID của đơn xuất kho</param>
    /// <returns>Thông tin đơn xuất kho</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get Export Inventory By ID")]
    [EndpointDescription("Lấy chi tiết đơn xuất kho theo ID. Chỉ Admin/Staff mới có quyền.")]
    public async Task<ActionResult<APIResponse>> GetExportInventoryById(ulong id)
    {
        try
        {
            var exportInventory = await _exportInventoryService.GetExportInventoryByIdAsync(id, GetCancellationToken());
            return SuccessResponse(exportInventory);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả đơn xuất kho với phân trang và filter theo MovementType
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="movementType">Loại xuất kho để filter (Sale, ReturnToVendor, Damage, Loss, Adjustment). Mặc định: tất cả</param>
    /// <returns>Danh sách đơn xuất kho có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get All Export Inventories (note.)")]
    [EndpointDescription("Lọc đơn xuất kho theo MovementType. Nếu không ghi ra, trả về tất cả. " +
                         "Mẫu: /api/ExportInventory?page=2&pageSize=20&movementType=Damage")]
    public async Task<ActionResult<APIResponse>> GetAllExportInventories(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? movementType = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var exportInventories = await _exportInventoryService.GetAllExportInventoriesAsync(
                page, pageSize, movementType, GetCancellationToken());
            return SuccessResponse(exportInventories);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách số lô hoặc số sê-ri có sẵn theo ProductId
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <returns>Danh sách số lô (nếu sản phẩm không yêu cầu serial) hoặc danh sách số sê-ri (nếu yêu cầu serial)</returns>
    [HttpGet("identity-numbers/{productId}")]
    [Authorize]
    [EndpointSummary("Get Identity Numbers By ProductId")]
    [EndpointDescription("Lấy danh sách số lô (kèm số lượng còn lại) hoặc số sê-ri (kèm số lô) có sẵn trong kho theo ProductId. " +
                         "Tất cả người dùng đã đăng nhập đều có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetIdentityNumbers(ulong productId)
    {
        try
        {
            var identityNumbers = await _exportInventoryService.GetIdentityNumbersAsync(productId, GetCancellationToken());
            return SuccessResponse(identityNumbers);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách số lô hoặc số sê-ri đã xuất kho theo OrderDetailId
    /// </summary>
    /// <param name="orderDetailId">ID của chi tiết đơn hàng</param>
    /// <returns>Danh sách số lô hoặc số sê-ri đã xuất kho cho đơn hàng</returns>
    [HttpGet("exported-identity-numbers/{orderDetailId}")]
    [Authorize]
    [EndpointSummary("Get Exported Identity Numbers By OrderDetailId")]
    [EndpointDescription("Lấy danh sách số lô (kèm số lượng) hoặc số sê-ri (kèm số lô) đã được xuất kho theo OrderDetailId. " +
                         "Tất cả người dùng đã đăng nhập đều có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetAllIdentityNumbersExportedByOrderDetailId(ulong orderDetailId)
    {
        try
        {
            var identityNumbers = await _exportInventoryService.GetAllIdentityNumbersExportedByOrderDetailIdAsync(orderDetailId, GetCancellationToken());
            return SuccessResponse(identityNumbers);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
