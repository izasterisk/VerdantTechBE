using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICustomerVendorConversationsRepository
{
    Task SendNewMessageAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, List<MediaLink> images, CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetAllMessageImagesByIdAsync(ulong id, CancellationToken cancellationToken);
    Task<CustomerVendorConversation> GetOrCreateConversationByUserIdAsync(ulong customerId, ulong vendorId, UserRole senderRole, CancellationToken cancellationToken = default);
    Task<CustomerVendorMessage> GetNewestMessageByConversationIdAsync(ulong conversationId,
        CancellationToken cancellationToken = default);
    Task<(List<CustomerVendorMessage>, int totalCount)> GetAllMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<MediaLink>> GetMediaLinksByOwnerIdsAsync(List<ulong> ownerIds,
        CancellationToken cancellationToken = default);
    Task<(List<CustomerVendorConversation>, int totalCount)> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task ValidateProductBelongsToVendor(ulong productId, ulong vendorId, CancellationToken cancellationToken = default);
}