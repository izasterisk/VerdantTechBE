using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Request;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using DAL.Data;

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
    /// Tạo yêu cầu mới (hỗ trợ hoặc hoàn tiền)
    /// </summary>
    /// <param name="dto">Thông tin yêu cầu cần tạo</param>
    /// <returns>Thông tin yêu cầu đã tạo</returns>
    [HttpPost]
    [Authorize]
    [EndpointSummary("Create New Request")]
    [EndpointDescription("Tạo yêu cầu mới. RequestType: RefundRequest, SupportRequest")]
    public async Task<ActionResult<APIResponse>> CreateRequest([FromBody] RequestCreateDTO dto)
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
    /// Xử lý yêu cầu (Admin/Staff)
    /// </summary>
    /// <param name="requestId">ID của yêu cầu</param>
    /// <param name="dto">Thông tin xử lý yêu cầu</param>
    /// <returns>Thông tin yêu cầu đã cập nhật</returns>
    [HttpPatch("{requestId}/process")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Process Request")]
    [EndpointDescription("Xử lý yêu cầu: cập nhật trạng thái hoặc thêm ghi chú phản hồi. " +
                         "Status có thể là: InReview, Approved, Rejected, Cancelled. " +
                         "Không thể cập nhật về Pending hoặc Completed.")]
    public async Task<ActionResult<APIResponse>> ProcessRequest(ulong requestId, [FromBody] RequestProcessDTO dto)
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

    /// <summary>
    /// Gửi tin nhắn mới cho yêu cầu (User)
    /// </summary>
    /// <param name="requestId">ID của yêu cầu</param>
    /// <param name="dto">Nội dung tin nhắn</param>
    /// <returns>Thông tin yêu cầu đã cập nhật</returns>
    [HttpPost("{requestId}/message")]
    [Authorize]
    [EndpointSummary("Send New Request Message")]
    [EndpointDescription("Gửi tin nhắn mới cho yêu cầu. Tối đa 3 tin nhắn. " +
                         "Chỉ có thể gửi tin nhắn mới khi tất cả tin nhắn trước đó đã được phản hồi.")]
    public async Task<ActionResult<APIResponse>> SendNewRequestMessage(ulong requestId, [FromBody] RequestMessageCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var request = await _requestService.SendNewRequestMessageAsync(userId, requestId, dto, GetCancellationToken());
            return SuccessResponse(request, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin yêu cầu theo ID
    /// </summary>
    /// <param name="requestId">ID của yêu cầu</param>
    /// <returns>Thông tin yêu cầu</returns>
    [HttpGet("{requestId}")]
    [Authorize]
    [EndpointSummary("Get Request By ID")]
    [EndpointDescription("Lấy thông tin chi tiết của yêu cầu theo ID")]
    public async Task<ActionResult<APIResponse>> GetRequestById(ulong requestId)
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
    /// Lấy danh sách yêu cầu của người dùng hiện tại
    /// </summary>
    /// <returns>Danh sách yêu cầu</returns>
    [HttpGet("my-requests")]
    [Authorize]
    [EndpointSummary("Get My Requests")]
    [EndpointDescription("Lấy danh sách tất cả yêu cầu của người dùng hiện tại")]
    public async Task<ActionResult<APIResponse>> GetMyRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _requestService.GetAllRequestByUserIdAsync(userId, GetCancellationToken());
            return SuccessResponse(requests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả yêu cầu với phân trang và filter (Admin/Staff)
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="requestType">Loại yêu cầu: RefundRequest, SupportRequest</param>
    /// <param name="requestStatus">Trạng thái: Pending, InReview, Approved, Rejected, Completed, Cancelled</param>
    /// <returns>Danh sách yêu cầu có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get All Requests")]
    [EndpointDescription("Lấy danh sách tất cả yêu cầu với phân trang và filter. Chỉ Admin/Staff có quyền.")]
    public async Task<ActionResult<APIResponse>> GetAllRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestType? requestType = null,
        [FromQuery] RequestStatus? requestStatus = null)
    {
        try
        {
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var requests = await _requestService.GetAllRequestByFiltersAsync(page, pageSize, requestType, requestStatus, GetCancellationToken());
            return SuccessResponse(requests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
