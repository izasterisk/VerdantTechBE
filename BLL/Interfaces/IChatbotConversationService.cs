using BLL.DTO;
using BLL.DTO.ChatbotConversations;

namespace BLL.Interfaces;

public interface IChatbotConversationService
{
    Task<PagedResponse<ChatbotConversationsResponseDTO>> GetAllChatbotConversationByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<ChatbotMessagesResponseDTO>> GetAllChatbotMessageByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteConversationAsync(ulong conversationId, ulong userId, CancellationToken ct = default);
}