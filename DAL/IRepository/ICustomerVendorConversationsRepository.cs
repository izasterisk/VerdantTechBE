using DAL.Data.Models;

namespace DAL.IRepository;

public interface ICustomerVendorConversationsRepository
{
    Task CreateConversationAsync(CustomerVendorConversation conversation,
        CustomerVendorMessage message, CancellationToken cancellationToken = default);
}