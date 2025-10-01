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
    /// Tạo đơn hàng mới cho người dùng hiện tại (lấy userId từ JWT)
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng</param>
    /// <returns>Đơn hàng vừa tạo</returns>
    [HttpPost]
    [Authorize]
    [EndpointSummary("Create Order")]
    [EndpointDescription("Tạo đơn hàng mới cho user hiện tại (userId lấy từ token).")]
    public async Task<ActionResult<APIResponse>> CreateOrder([FromBody] OrderCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.CreateOrderAsync(userId, dto, GetCancellationToken());
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
    [EndpointDescription("Cập nhật một phần đơn hàng. Nếu Quantity của sản phẩm = 0 thì sản phẩm đó sẽ bị xóa khỏi order. " +
                         "Nếu xóa hết item thì sẽ tự động xóa luôn order, trả về 204 No Content. " +
                         "Nếu muốn thêm sản phẩm mới vào order thì thêm bình thường nhưng phải để Id = 0. " +
                         "Lưu ý: Xóa là HARD DELETE, không thể khôi phục.")]
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
