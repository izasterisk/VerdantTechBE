using BLL.DTO.CustomerVendorConversation;

namespace BLL.Interfaces;

public interface ICustomerVendorConversationsService
{
    Task<CustomerVendorConversationReponseDTO> CreateConversationAsync(ulong customerId, CustomerVendorConversationCreateDTO dto, CancellationToken cancellationToken = default);
}