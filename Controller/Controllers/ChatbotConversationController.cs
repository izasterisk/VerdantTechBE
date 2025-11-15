using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.ChatbotConversations;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChatbotConversationController : BaseController
{
    private readonly IChatbotConversationService _chatbotConversationService;
    
    public ChatbotConversationController(IChatbotConversationService chatbotConversationService)
    {
        _chatbotConversationService = chatbotConversationService;
    }

    /// <summary>
    /// Tạo cuộc hội thoại chatbot mới với tin nhắn đầu tiên
    /// </summary>
    /// <param name="dto">Thông tin tin nhắn đầu tiên</param>
    /// <returns>Thông tin cuộc hội thoại và tin nhắn đã tạo</returns>
    [HttpPost]
    [EndpointSummary("Create New Chatbot Conversation")]
    [EndpointDescription("Tạo cuộc hội thoại chatbot mới với tin nhắn đầu tiên. User ID được lấy từ token.")]
    public async Task<ActionResult<APIResponse>> CreateNewChatbotConversation([FromBody] ChatbotMessageCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatbotConversationService.CreateNewChatbotConversationAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Gửi tin nhắn mới trong cuộc hội thoại hiện có
    /// </summary>
    /// <param name="conversationId">ID của cuộc hội thoại</param>
    /// <param name="dto">Thông tin tin nhắn</param>
    /// <returns>Thông tin tin nhắn đã gửi</returns>
    [HttpPost("{conversationId}/message")]
    [EndpointSummary("Send New Message")]
    [EndpointDescription("Gửi tin nhắn mới trong cuộc hội thoại đã tồn tại. User ID được lấy từ token.")]
    public async Task<ActionResult<APIResponse>> SendNewMessage(ulong conversationId, [FromBody] ChatbotMessageCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatbotConversationService.SendNewMessageAsync(userId, conversationId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật thông tin cuộc hội thoại chatbot
    /// </summary>
    /// <param name="conversationId">ID của cuộc hội thoại</param>
    /// <returns>Thông tin cuộc hội thoại đã cập nhật</returns>
    [HttpPatch("{conversationId}")]
    [EndpointSummary("Update Chatbot Conversation")]
    [EndpointDescription("Cập nhật thông tin cuộc hội thoại chatbot (title, context, is_active).")]
    public async Task<ActionResult<APIResponse>> UpdateChatbotConversation(ulong conversationId)
    {
        try
        {
            var result = await _chatbotConversationService.UpdateChatbotConversationAsync(conversationId, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả cuộc hội thoại của người dùng hiện tại
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách cuộc hội thoại có phân trang</returns>
    [HttpGet]
    [EndpointSummary("Get All Conversations")]
    [EndpointDescription("Lấy danh sách tất cả cuộc hội thoại của người dùng hiện tại với phân trang. User ID được lấy từ token.")]
    public async Task<ActionResult<APIResponse>> GetAllChatbotConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var userId = GetCurrentUserId();
            var result = await _chatbotConversationService.GetAllChatbotConversationByUserIdAsync(userId, page, pageSize, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tin nhắn trong cuộc hội thoại
    /// </summary>
    /// <param name="conversationId">ID của cuộc hội thoại</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách tin nhắn có phân trang</returns>
    [HttpGet("{conversationId}/messages")]
    [EndpointSummary("Get All Messages in Conversation")]
    [EndpointDescription("Lấy danh sách tin nhắn trong cuộc hội thoại với phân trang.")]
    public async Task<ActionResult<APIResponse>> GetAllChatbotMessages(ulong conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var result = await _chatbotConversationService.GetAllChatbotMessageByConversationIdAsync(conversationId, page, pageSize, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
