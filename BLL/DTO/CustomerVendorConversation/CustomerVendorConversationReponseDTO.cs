using BLL.DTO.User;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorConversationReponseDTO
{
    public ulong Id { get; set; }
    
    public UserResponseDTO Customer { get; set; } = null!;
    
    public UserResponseDTO Vendor { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
}