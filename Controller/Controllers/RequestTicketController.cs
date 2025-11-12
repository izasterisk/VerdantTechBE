using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Request;
using DAL.Data;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class RequestTicketController : BaseController
{
    private readonly IRequestService _requestService;
    
    public RequestTicketController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Tạo yêu cầu hỗ trợ hoặc yêu cầu hoàn tiền mới
    /// </summary>
    /// <param name="dto">Thông tin yêu cầu</param>
    /// <returns>Thông tin request ticket đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Customer,Vendor")]
    [EndpointSummary("Create Request Ticket")]
    [EndpointDescription("Tạo request ticket mới (support request hoặc refund request).")]
    public async Task<ActionResult<APIResponse>> CreateRequestTicket([FromBody] RequestCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var request = await _requestService.CreateRequestAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(request, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết một request ticket
    /// </summary>
    /// <param name="requestId">ID của request ticket</param>
    /// <returns>Thông tin chi tiết request ticket</returns>
    [HttpGet("{requestId}")]
    [Authorize]
    [EndpointSummary("Get Request Ticket By ID")]
    [EndpointDescription("Lấy thông tin chi tiết một request ticket theo ID.")]
    public async Task<ActionResult<APIResponse>> GetRequestTicketById(ulong requestId)
    {
        try
        {
            var request = await _requestService.GetRequestByIdAsync(requestId, GetCancellationToken());
            return SuccessResponse(request);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả request tickets của một user
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <returns>Danh sách request tickets của user</returns>
    [HttpGet("user/{userId}")]
    [Authorize]
    [EndpointSummary("Get All Request Tickets By User")]
    [EndpointDescription("Lấy tất cả request tickets của một user cụ thể.")]
    public async Task<ActionResult<APIResponse>> GetAllRequestTicketsByUserId(ulong userId)
    {
        try
        {
            var requests = await _requestService.GetAllRequestByUserIdAsync(userId, GetCancellationToken());
            return SuccessResponse(requests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả request tickets với filter và phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="requestType">Loại request (RefundRequest hoặc SupportRequest)</param>
    /// <param name="requestStatus">Trạng thái (Pending, InReview, Approved, Rejected, Cancelled)</param>
    /// <returns>Danh sách request tickets có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get All Request Tickets")]
    [EndpointDescription("Lấy danh sách tất cả request tickets với filter và phân trang. Chỉ Admin/Staff mới có quyền. " +
                         "Mẫu: /api/RequestTicket?page=1&pageSize=10&requestType=RefundRequest&requestStatus=Pending")]
    public async Task<ActionResult<APIResponse>> GetAllRequestTickets(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestType? requestType = null,
        [FromQuery] RequestStatus? requestStatus = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var requests = await _requestService.GetAllRequestByFiltersAsync(
                page, pageSize, requestType, requestStatus, GetCancellationToken());
            return SuccessResponse(requests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xử lý request ticket (Staff/Admin)
    /// </summary>
    /// <param name="requestId">ID của request ticket</param>
    /// <param name="dto">Thông tin cập nhật (Status, ReplyNotes)</param>
    /// <returns>Thông tin request ticket đã được xử lý</returns>
    [HttpPut("{requestId}/process")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Process Request Ticket")]
    [EndpointDescription("Xử lý request ticket (cập nhật trạng thái và thêm ghi chú). Chỉ Admin/Staff mới có quyền. " +
                         "Các trạng thái hợp lệ: InReview, Approved, Rejected, Cancelled. " +
                         "Khi chuyển sang Approved/Rejected/Cancelled bắt buộc phải có ReplyNotes còn InReview thì không được có ReplyNotes.")]
    public async Task<ActionResult<APIResponse>> ProcessRequestTicket(ulong requestId, [FromBody] RequestUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var request = await _requestService.ProcessRequestAsync(staffId, requestId, dto, GetCancellationToken());
            return SuccessResponse(request);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
