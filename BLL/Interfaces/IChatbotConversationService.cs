using BLL.DTO;
using BLL.DTO.ChatbotConversations;

namespace BLL.Interfaces;

public interface IChatbotConversationService
{
    Task<SendNewMessageResponseDTO> CreateNewChatbotConversationAsync(ulong userId, ChatbotMessageCreateDTO dto, CancellationToken cancellationToken = default);
    Task<ChatbotMessagesResponseDTO> SendNewMessageAsync(ulong userId, ulong conversationId, ChatbotMessageCreateDTO dto, CancellationToken cancellationToken = default);
    Task<ChatbotConversationsResponseDTO> UpdateChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default);
    Task<PagedResponse<ChatbotConversationsResponseDTO>> GetAllChatbotConversationByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<ChatbotMessagesResponseDTO>> GetAllChatbotMessageByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
}