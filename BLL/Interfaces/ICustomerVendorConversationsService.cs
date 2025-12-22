using BLL.DTO.CustomerVendorConversation;
using DAL.Data;

namespace BLL.Interfaces;

public interface ICustomerVendorConversationsService
{
    Task<CustomerVendorConversationReponseDTO> CreateConversationAsync(ulong customerId, CustomerVendorConversationCreateDTO dto, CancellationToken cancellationToken = default);
    Task<CustomerVendorMessageResponseDTO> SendNewMessageAsync(ulong userId, UserRole role, ulong conversationId, CustomerVendorMessageCreateDTO dto, CancellationToken cancellationToken = default);
}