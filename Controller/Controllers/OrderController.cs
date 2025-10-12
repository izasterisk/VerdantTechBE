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
    [EndpointDescription("Tạo preview đơn hàng với các thông tin chi tiết về sản phẩm, địa chỉ giao hàng, " +
                         "phí vận chuyển và thời gian giao hàng dự kiến. Preview này sẽ được cache trong 10 phút " +
                         "để sử dụng cho việc tạo đơn hàng thực tế sau này.")]
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
}
