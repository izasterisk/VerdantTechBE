using BLL.DTO;
using BLL.DTO.CustomerVendorConversation;
using DAL.Data;

namespace BLL.Interfaces;

public interface ICustomerVendorConversationsService
{
    Task<PagedResponse<CustomerVendorMessageResponseDTO>> GetAllMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerVendorConversationReponseDTO>> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
}