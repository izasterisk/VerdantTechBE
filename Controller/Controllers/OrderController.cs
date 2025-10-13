using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Order;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;
    
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Tạo preview đơn hàng để xem trước thông tin trước khi tạo đơn
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng preview</param>
    /// <returns>Thông tin preview đơn hàng bao gồm tính toán phí vận chuyển và thời gian giao hàng</returns>
    [HttpPost("preview")]
    [EndpointSummary("Create Order Preview")]
    public async Task<ActionResult<APIResponse>> CreateOrderPreview([FromBody] OrderPreviewCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var preview = await _orderService.CreateOrderPreviewAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(preview, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Tạo đơn hàng thực tế từ order preview đã được tạo trước đó
    /// </summary>
    /// <param name="orderPreviewId">ID của order preview</param>
    /// <param name="dto">Thông tin đơn hàng bao gồm dịch vụ vận chuyển đã chọn</param>
    /// <returns>Thông tin đơn hàng đã được tạo</returns>
    [HttpPost("{orderPreviewId:guid}")]
    [EndpointSummary("Create Order")]
    public async Task<ActionResult<APIResponse>> CreateOrder([FromRoute] Guid orderPreviewId, [FromBody] OrderCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var order = await _orderService.CreateOrderAsync(orderPreviewId, dto, GetCancellationToken());
            return SuccessResponse(order, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
