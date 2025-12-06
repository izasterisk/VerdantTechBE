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

    [HttpDelete("{conversationId}")]
    [EndpointSummary("Soft delete conversation")]
    [EndpointDescription("Xóa mềm cuộc hội thoại của người dùng hiện tại. User ID được lấy từ token.")]
    public async Task<ActionResult<APIResponse>> SoftDeleteConversation(ulong conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatbotConversationService.SoftDeleteConversationAsync(
                conversationId, userId, GetCancellationToken()
            );

            return SuccessResponse("Đã xóa cuộc trò chuyện.");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}
