using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.DTO;
using BLL.DTO.Order;
using BLL.Helpers.Order;
using BLL.Interfaces;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class OrderController : BaseController
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Tạo preview đơn hàng (bước 1: xem trước giá, phí ship, tổng tiền)
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng preview</param>
    /// <returns>Order preview với các tùy chọn vận chuyển, được cache trong 10 phút</returns>
    [HttpPost("preview")]
    [Authorize]
    [EndpointSummary("Create Order Preview")]
    [EndpointDescription("Tạo preview đơn hàng để xem trước giá, phí ship và các tùy chọn vận chuyển. Preview sẽ được cache trong 10 phút rồi biến mất hoàn toàn vì không được lưu trong DB.")]
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
    /// Tạo đơn hàng thật từ preview (bước 2: xác nhận và lưu vào database)
    /// </summary>
    /// <param name="orderPreviewId">ID của order preview (lấy từ response của CreateOrderPreview)</param>
    /// <param name="dto">Chỉ cần ShippingDetailId - ID phương thức vận chuyển được chọn</param>
    /// <returns>Đơn hàng đã được lưu vào database</returns>
    [HttpPost("{orderPreviewId}")]
    [Authorize]
    [EndpointSummary("Create Order From Preview")]
    [EndpointDescription("Tạo đơn hàng thật từ preview đã cache, nhận vào Guid orderPreviewId. Preview phải còn hợp lệ (chưa quá 10 phút).")]
    public async Task<ActionResult<APIResponse>> CreateOrder(Guid orderPreviewId, [FromBody] OrderCreateDTO dto)
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

    /// <summary>
    /// Cập nhật đơn hàng theo cơ chế PATCH (chỉ map các trường được gửi)
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <param name="dto">Các trường cần cập nhật</param>
    /// <returns>Đơn hàng sau cập nhật hoặc 204 nếu đơn bị xóa vì hết item</returns>
    [HttpPatch("{orderId}")]
    [Authorize]
    [EndpointSummary("Update Order (PATCH)")]
    [EndpointDescription("Nếu truyền CancelledReason đồng nghĩa với xác nhận hủy đơn. Không thể hủy đơn khi trạng thái đã là Shipped hoặc Delivered.")]
    public async Task<ActionResult<APIResponse>> UpdateOrder(ulong orderId, [FromBody] OrderUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var order = await _orderService.UpdateOrderAsync(orderId, dto, GetCancellationToken());
            return SuccessResponse(order);
        }
        catch (OrderHelper.OrderDeletedException)
        {
            // Đơn hàng đã bị xóa do không còn item nào
            return SuccessResponse(null, HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng theo ID
    /// </summary>
    /// <param name="orderId">ID đơn hàng</param>
    /// <returns>Chi tiết đơn hàng</returns>
    [HttpGet("{orderId}")]
    [Authorize]
    [EndpointSummary("Get Order By ID")]
    [EndpointDescription("Trả về thông tin chi tiết của đơn hàng theo ID.")]
    public async Task<ActionResult<APIResponse>> GetOrderById(ulong orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByOrderIdAsync(orderId, GetCancellationToken());
            if (order == null)
                return ErrorResponse($"Không tìm thấy đơn hàng với ID {orderId}", HttpStatusCode.NotFound);

            return SuccessResponse(order);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    
    /// <summary>
    /// Lấy tất cả đơn hàng của người dùng hiện tại (userId lấy từ JWT)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Get My Orders")]
    [EndpointDescription("Trả về danh sách đơn hàng của user đang đăng nhập.")]
    public async Task<ActionResult<APIResponse>> GetMyOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetAllOrdersByUserIdAsync(userId, GetCancellationToken());
            return SuccessResponse(orders);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// (Admin/Staff) Lấy tất cả đơn hàng của một người dùng theo ID
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get Orders By User ID")]
    [EndpointDescription("Chỉ Admin/Staff: trả về danh sách đơn hàng của user theo ID.")]
    public async Task<ActionResult<APIResponse>> GetOrdersByUserId(ulong userId)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersByUserIdAsync(userId, GetCancellationToken());
            return SuccessResponse(orders);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
