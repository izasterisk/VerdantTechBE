// using Microsoft.AspNetCore.Mvc;
// using BLL.Interfaces;
// using BLL.DTO;
// using BLL.DTO.Order;
// using System.Net;
// using Microsoft.AspNetCore.Authorization;
//
// namespace Controller.Controllers;
//
// [Route("api/[controller]")]
// [Authorize]
// public class OrderController : BaseController
// {
//     private readonly IOrderService _orderService;
//     
//     public OrderController(IOrderService orderService)
//     {
//         _orderService = orderService;
//     }
//
//     /// <summary>
//     /// Tạo preview đơn hàng để xem trước thông tin trước khi tạo đơn
//     /// </summary>
//     /// <param name="dto">Thông tin đơn hàng preview</param>
//     /// <returns>Thông tin preview đơn hàng bao gồm tính toán phí vận chuyển và thời gian giao hàng</returns>
//     [HttpPost("preview")]
//     [EndpointSummary("Create Order Preview")]
//     public async Task<ActionResult<APIResponse>> CreateOrderPreview([FromBody] OrderPreviewCreateDTO dto)
//     {
//         var validationResult = ValidateModel();
//         if (validationResult != null) return validationResult;
//
//         try
//         {
//             var userId = GetCurrentUserId();
//             var preview = await _orderService.CreateOrderPreviewAsync(userId, dto, GetCancellationToken());
//             return SuccessResponse(preview, HttpStatusCode.Created);
//         }
//         catch (Exception ex)
//         {
//             return HandleException(ex);
//         }
//     }
//
//     /// <summary>
//     /// Tạo đơn hàng thực tế từ order preview đã được tạo trước đó
//     /// </summary>
//     /// <param name="orderPreviewId">ID của order preview</param>
//     /// <param name="dto">Thông tin đơn hàng bao gồm dịch vụ vận chuyển đã chọn</param>
//     /// <returns>Thông tin đơn hàng đã được tạo</returns>
//     [HttpPost("{orderPreviewId:guid}")]
//     [EndpointSummary("Create Order")]
//     public async Task<ActionResult<APIResponse>> CreateOrder([FromRoute] Guid orderPreviewId, [FromBody] OrderCreateDTO dto)
//     {
//         var validationResult = ValidateModel();
//         if (validationResult != null) return validationResult;
//
//         try
//         {
//             var order = await _orderService.CreateOrderAsync(orderPreviewId, dto, GetCancellationToken());
//             return SuccessResponse(order, HttpStatusCode.Created);
//         }
//         catch (Exception ex)
//         {
//             return HandleException(ex);
//         }
//     }
//
//     /// <summary>
//     /// Lấy danh sách tất cả đơn hàng với phân trang và lọc theo trạng thái
//     /// </summary>
//     /// <param name="page">Số trang (mặc định là 1)</param>
//     /// <param name="pageSize">Số lượng đơn hàng trên mỗi trang (mặc định là 10)</param>
//     /// <param name="status">Trạng thái đơn hàng để lọc (không bắt buộc)</param>
//     /// <returns>Danh sách đơn hàng với thông tin phân trang</returns>
//     [HttpGet]
//     [EndpointSummary("Get All Orders")]
//     [EndpointDescription("GET toàn bộ Order theo Status (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled, Refunded). Hoặc không nhập status cũng được, trả về toàn bộ.")]
//     public async Task<ActionResult<APIResponse>> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
//     {
//         try
//         {
//             var orders = await _orderService.GetAllOrdersAsync(page, pageSize, status, GetCancellationToken());
//             return SuccessResponse(orders, HttpStatusCode.OK);
//         }
//         catch (Exception ex)
//         {
//             return HandleException(ex);
//         }
//     }
// }
