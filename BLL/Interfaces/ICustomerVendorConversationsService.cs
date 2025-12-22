using BLL.DTO;
using BLL.DTO.CustomerVendorConversation;
using DAL.Data;

namespace BLL.Interfaces;

public interface ICustomerVendorConversationsService
{
    Task<CustomerVendorConversationReponseDTO> CreateConversationAsync(ulong customerId, CustomerVendorConversationCreateDTO dto, CancellationToken cancellationToken = default);
    Task<CustomerVendorMessageResponseDTO> SendNewMessageAsync(ulong userId, UserRole role, ulong conversationId, CustomerVendorMessageCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerVendorMessageResponseDTO>> GetAllMessagesByConversationIdAsync(ulong conversationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResponse<CustomerVendorListConversationsReponseDTO>> GetAllConversationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
}