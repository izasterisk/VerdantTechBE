using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class CustomerVendorConversationsRepository : ICustomerVendorConversationsRepository
{
    private readonly IRepository<CustomerVendorConversation> _customerVendorConversationsRepository;
    private readonly IRepository<CustomerVendorMessage> _customerVendorMessageRepository;
    private readonly VerdantTechDbContext _dbContext;

    public CustomerVendorConversationsRepository(
        IRepository<CustomerVendorConversation> customerVendorConversationsRepository,
        IRepository<CustomerVendorMessage> customerVendorMessageRepository, VerdantTechDbContext dbContext)
    {
        _customerVendorConversationsRepository = customerVendorConversationsRepository;
        _customerVendorMessageRepository = customerVendorMessageRepository;
        _dbContext = dbContext;
    }

    public async Task CreateConversationAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, CancellationToken cancellationToken = default)
    {
        conversation.StartedAt = DateTime.UtcNow;
        conversation.LastMessageAt = DateTime.UtcNow;
        var createdConversation = await _customerVendorConversationsRepository.CreateAsync(conversation, cancellationToken);
        
        message.ConversationId = createdConversation.Id;
        message.IsRead = true;
        message.CreatedAt = DateTime.UtcNow;
        await _customerVendorMessageRepository.CreateAsync(message, cancellationToken);
    }
}