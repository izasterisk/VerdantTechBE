using DAL.Data.Models;

namespace DAL.IRepository;

public interface IChatbotConversationRepository
{
    Task<(ChatbotConversation conversation, ChatbotMessage message)> CreateConversationWithTransactionAsync(ChatbotConversation conversation, ChatbotMessage chatbotMessage, CancellationToken cancellationToken = default);
    Task<bool> IsConversationBelongToUserAsync(ulong conversationId, ulong userId, CancellationToken cancellationToken = default);
    Task<(List<ChatbotConversation>, int totalCount)> GetAllChatbotConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<ChatbotMessage>, int totalCount)> GetAllChatbotMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ChatbotMessage> CreateNewChatbotMessageAsync(ChatbotMessage chatbotMessage, CancellationToken cancellationToken = default);
    Task<ChatbotConversation> UpdateChatbotConversationAsync(ChatbotConversation conversation, CancellationToken cancellationToken = default);
    Task<ChatbotConversation> GetActiveChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default);
}