using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class ChatbotConversationRepository : IChatbotConversationRepository
{
    private readonly IRepository<ChatbotMessage> _chatbotMessageRepository;
    private readonly IRepository<ChatbotConversation> _chatbotConversationRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public ChatbotConversationRepository(IRepository<ChatbotMessage> chatbotMessageRepository, IRepository<ChatbotConversation> chatbotConversationRepository, VerdantTechDbContext dbContext)
    {
        _chatbotMessageRepository = chatbotMessageRepository;
        _chatbotConversationRepository = chatbotConversationRepository;
        _dbContext = dbContext;
    }
    
    
    
    
    
    public async Task<ChatbotConversation> GetActiveChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.GetAsync(c => c.Id == conversationId && c.IsActive, 
            true, cancellationToken) ?? 
               throw new KeyNotFoundException("Cuộc trò chuyện không tồn tại hoặc đã bị xóa.");
    }
    
    
    public async Task<(List<ChatbotConversation>, int totalCount)> GetAllChatbotConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            filter: c => c.CustomerId == userId && c.IsActive == true,
            useNoTracking: true,
            //orderBy: query => query.OrderByDescending(c => c.Id),
            orderBy: query => query.OrderBy(m => m.StartedAt),
            includeFunc: query => query.Include(c => c.Customer),
            cancellationToken
        );
    }
    
    public async Task<(List<ChatbotMessage>, int totalCount)> GetAllChatbotMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _chatbotMessageRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            filter: m => m.ConversationId == conversationId && m.Conversation.IsActive == true,
            useNoTracking: true,
            //orderBy: query => query.OrderByDescending(m => m.Id),
            orderBy: query => query.OrderBy(m => m.CreatedAt),
            includeFunc: query => query.Include(m => m.Conversation),
            cancellationToken
        );
    }

    public async Task SoftDeleteConversationAsync(ulong conversationId, CancellationToken cancellationToken = default)
    {
        var convo = await _chatbotConversationRepository.GetAsync(
            c => c.Id == conversationId && c.IsActive == true,
            false,
            cancellationToken
        );

        if (convo == null)
            throw new KeyNotFoundException("Cuộc hội thoại không tồn tại.");

        convo.IsActive = false;

        await _chatbotConversationRepository.UpdateAsync(convo, cancellationToken);
    }
}