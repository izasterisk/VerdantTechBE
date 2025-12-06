using DAL.Data.Models;

namespace DAL.IRepository;

public interface IChatbotConversationRepository
{
    Task<ChatbotConversation> GetActiveChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default);
    Task<(List<ChatbotConversation>, int totalCount)> GetAllChatbotConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<ChatbotMessage>, int totalCount)> GetAllChatbotMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task SoftDeleteConversationAsync(ulong conversationId, CancellationToken cancellationToken = default);
}