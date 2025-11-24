using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Order;
using System.Net;
using Microsoft.AspNetCore.Authorization;

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
    /// Tạo preview đơn hàng để xem trước thông tin trước khi tạo đơn
    /// </summary>
    /// <param name="dto">Thông tin đơn hàng preview</param>
    /// <returns>Thông tin preview đơn hàng bao gồm tính toán phí vận chuyển và thời gian giao hàng</returns>
    [HttpPost("preview")]
    [Authorize]
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
    [Authorize]
    [EndpointSummary("Create Order")]
    public async Task<ActionResult<APIResponse>> CreateOrder([FromRoute] Guid orderPreviewId,
        [FromBody] OrderCreateDTO dto)
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
    /// Xuất kho và gán số serial/số lô cho sản phẩm trong đơn hàng trước khi ship
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="dtos">Danh sách sản phẩm với số serial hoặc số lô</param>
    /// <returns>Thông tin đơn hàng sau khi xuất kho</returns>
    [HttpPost("{orderId:long}/ship")]
    [Authorize(Roles = "Admin, Staff")]
    [EndpointSummary("Ship Order")]
    [EndpointDescription("Gửi sản phẩm đi, nếu là máy móc category ID = 24/25/28/29 thì chỉ cần nhập số seri, tất cả các loại khác thì chỉ cần nhập số lô không cần seri.")]
    public async Task<ActionResult<APIResponse>> ShipOrder([FromRoute] ulong orderId,
        [FromBody] List<OrderDetailsShippingDTO> dtos)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var order = await _orderService.ShipOrderAsync(staffId, orderId, dtos, GetCancellationToken());
            return SuccessResponse(order, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng hoặc hủy đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="dto">Thông tin cập nhật đơn hàng (trạng thái mới và lý do hủy nếu có)</param>
    /// <returns>Thông tin đơn hàng sau khi cập nhật</returns>
    [HttpPut("{orderId:long}")]
    [Authorize(Roles = "Admin, Staff")]
    [EndpointSummary("Process Order")]
    [EndpointDescription(
        "Cập nhật trạng thái đơn hàng: Paid, Processing, Delivered, Cancelled, Refunded. Hoặc hủy đơn hàng với lý do.")]
    public async Task<ActionResult<APIResponse>> ProcessOrder([FromRoute] ulong orderId, [FromBody] OrderUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var order = await _orderService.ProcessOrderAsync(staffId, orderId, dto, GetCancellationToken());
            return SuccessResponse(order, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết một đơn hàng theo ID
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <returns>Thông tin chi tiết đơn hàng</returns>
    [HttpGet("{orderId:long}")]
    [Authorize]
    [EndpointSummary("Get Order By ID")]
    public async Task<ActionResult<APIResponse>> GetOrderById([FromRoute] ulong orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, GetCancellationToken());
            return SuccessResponse(order, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả đơn hàng với phân trang và lọc theo trạng thái
    /// </summary>
    /// <param name="page">Số trang (mặc định là 1)</param>
    /// <param name="pageSize">Số lượng đơn hàng trên mỗi trang (mặc định là 10)</param>
    /// <param name="status">Trạng thái đơn hàng để lọc (không bắt buộc)</param>
    /// <returns>Danh sách đơn hàng với thông tin phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin, Staff")]
    [EndpointSummary("Get All Orders")]
    [EndpointDescription(
        "GET toàn bộ Order theo Status (Pending, Paid, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded). Hoặc không nhập status cũng được, trả về toàn bộ.")]
    public async Task<ActionResult<APIResponse>> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync(page, pageSize, status, GetCancellationToken());
            return SuccessResponse(orders, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đơn hàng của một khách hàng cụ thể với phân trang
    /// </summary>
    /// <param name="userId">ID của khách hàng</param>
    /// <param name="page">Số trang (mặc định là 1)</param>
    /// <param name="pageSize">Số lượng đơn hàng trên mỗi trang (mặc định là 10)</param>
    /// <returns>Danh sách đơn hàng của khách hàng với thông tin phân trang</returns>
    [HttpGet("user/{userId:long}")]
    [Authorize]
    [EndpointSummary("Get All Orders By User ID")]
    public async Task<ActionResult<APIResponse>> GetAllOrdersByUserId([FromRoute] ulong userId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersByUserIdAsync(userId, page, pageSize, GetCancellationToken());
            return SuccessResponse(orders, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}