using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICustomerVendorConversationsRepository
{
    Task CreateConversationAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, List<MediaLink> images, CancellationToken cancellationToken = default);
    Task<CustomerVendorConversation> GetConversationWithRelationByIdAsync(ulong conversationId,
        CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetAllMessageImagesByIdAsync(ulong id, CancellationToken cancellationToken);
    Task<CustomerVendorConversation> GetConversationByIdAsync(ulong conversationId,
        CancellationToken cancellationToken = default);
    Task SendNewMessageAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, List<MediaLink> images, CancellationToken cancellationToken = default);
    Task<CustomerVendorMessage> GetNewestMessageByConversationIdAsync(ulong conversationId,
        CancellationToken cancellationToken = default);
}