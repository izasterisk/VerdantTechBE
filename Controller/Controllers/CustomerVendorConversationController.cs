using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.DTO;
using BLL.DTO.CustomerVendorConversation;
using BLL.Interfaces;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerVendorConversationController : BaseController
{
    private readonly ICustomerVendorConversationsService _service;
    
    public CustomerVendorConversationController(ICustomerVendorConversationsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Tạo cuộc hội thoại mới giữa customer và vendor
    /// </summary>
    /// <param name="dto">Thông tin cuộc hội thoại và tin nhắn đầu tiên</param>
    /// <returns>Thông tin cuộc hội thoại đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Create Customer-Vendor Conversation")]
    [EndpointDescription("Tạo cuộc hội thoại mới với tin nhắn đầu tiên. Có thể đính kèm tối đa 3 ảnh.")]
    public async Task<ActionResult<APIResponse>> CreateConversation([FromForm] CustomerVendorConversationCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.CreateConversationAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Gửi tin nhắn mới trong cuộc hội thoại
    /// </summary>
    /// <param name="conversationId">ID cuộc hội thoại</param>
    /// <param name="dto">Nội dung tin nhắn và ảnh đính kèm</param>
    /// <returns>Thông tin tin nhắn đã gửi</returns>
    [HttpPost("{conversationId}/messages")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Send Message in Conversation")]
    [EndpointDescription("Gửi tin nhắn mới trong cuộc hội thoại. Có thể đính kèm tối đa 3 ảnh.")]
    public async Task<ActionResult<APIResponse>> SendMessage(ulong conversationId, [FromForm] CustomerVendorMessageCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var result = await _service.SendNewMessageAsync(userId, role, conversationId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả tin nhắn trong cuộc hội thoại với phân trang
    /// </summary>
    /// <param name="conversationId">ID cuộc hội thoại</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng tin nhắn mỗi trang (mặc định: 20)</param>
    /// <returns>Danh sách tin nhắn với phân trang, tin nhắn mới nhất ở đầu</returns>
    [HttpGet("{conversationId}/messages")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Get All Messages by Conversation ID")]
    [EndpointDescription("Lấy tất cả tin nhắn trong cuộc hội thoại với phân trang. Tin nhắn mới nhất được sắp xếp trên cùng.")]
    public async Task<ActionResult<APIResponse>> GetAllMessagesByConversationId(
        ulong conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _service.GetAllMessagesByConversationIdAsync(conversationId, page, pageSize, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả cuộc hội thoại của người dùng với phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng cuộc hội thoại mỗi trang (mặc định: 20)</param>
    /// <returns>Danh sách cuộc hội thoại với phân trang, cuộc hội thoại mới nhất ở đầu</returns>
    [HttpGet("my-conversations")]
    [Authorize]
    [EndpointSummary("Get All Conversations by User ID")]
    [EndpointDescription("Lấy tất cả cuộc hội thoại của người dùng hiện tại với phân trang. Cuộc hội thoại có tin nhắn mới nhất được sắp xếp trên cùng.")]
    public async Task<ActionResult<APIResponse>> GetAllConversationsByUserId(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetAllConversationsByUserIdAsync(userId, page, pageSize, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
