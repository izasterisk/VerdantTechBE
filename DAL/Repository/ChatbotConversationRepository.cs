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
    
    public async Task<(ChatbotConversation conversation, ChatbotMessage message)> CreateConversationWithTransactionAsync(ChatbotConversation conversation, ChatbotMessage chatbotMessage, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            conversation.IsActive = true;
            conversation.StartedAt = DateTime.UtcNow;
            conversation.SessionId = Guid.NewGuid().ToString();
            var createdConversation = await _chatbotConversationRepository.CreateAsync(conversation, cancellationToken);
            
            chatbotMessage.ConversationId = createdConversation.Id;
            chatbotMessage.CreatedAt = DateTime.UtcNow;
            var createdMessage = await _chatbotMessageRepository.CreateAsync(chatbotMessage, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            return (createdConversation, createdMessage);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<ChatbotConversation> UpdateChatbotConversationAsync(ChatbotConversation conversation, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.UpdateAsync(conversation, cancellationToken);
    }
    
    public async Task<ChatbotConversation> GetActiveChatbotConversationAsync(ulong conversationId, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.GetAsync(c => c.Id == conversationId && c.IsActive, 
            true, cancellationToken) ?? 
               throw new KeyNotFoundException("Cuộc trò chuyện không tồn tại hoặc đã bị xóa.");
    }
    
    public async Task<ChatbotMessage> CreateNewChatbotMessageAsync(ChatbotMessage chatbotMessage, CancellationToken cancellationToken = default)
    {
        chatbotMessage.CreatedAt = DateTime.UtcNow;
        return await _chatbotMessageRepository.CreateAsync(chatbotMessage, cancellationToken);
    }
    
    public async Task<bool> IsConversationBelongToUserAsync(ulong conversationId, ulong userId, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.AnyAsync(
            filter: c => c.Id == conversationId && c.IsActive == true && c.CustomerId == userId 
                         && c.Customer.IsVerified == true && c.Customer.Status == UserStatus.Active,
            cancellationToken: cancellationToken);
    }
    
    public async Task<(List<ChatbotConversation>, int totalCount)> GetAllChatbotConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _chatbotConversationRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            filter: c => c.CustomerId == userId,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(c => c.Id),
            includeFunc: query => query.Include(c => c.Customer),
            cancellationToken
        );
    }
    
    public async Task<(List<ChatbotMessage>, int totalCount)> GetAllChatbotMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _chatbotMessageRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            filter: m => m.ConversationId == conversationId,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(m => m.Id),
            includeFunc: query => query.Include(m => m.Conversation),
            cancellationToken
        );
    }
}