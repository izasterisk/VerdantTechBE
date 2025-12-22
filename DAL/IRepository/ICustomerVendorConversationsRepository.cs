using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICustomerVendorConversationsRepository
{
    Task CreateConversationAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, List<MediaLink> images, CancellationToken cancellationToken = default);
    Task<CustomerVendorConversation> GetConversationWithRelationByIdAsync(ulong conversationId,
        CancellationToken cancellationToken = default);
}