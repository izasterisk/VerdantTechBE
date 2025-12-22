using BLL.DTO.User;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorConversationReponseDTO
{
    public ulong Id { get; set; }

    // public ulong CustomerId { get; set; }
    public UserResponseDTO Customer { get; set; } = null!;

    // public ulong VendorId { get; set; }
    public UserResponseDTO Vendor { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
    
    public List<CustomerVendorMessageResponseDTO> CustomerVendorMessages { get; set; } = new();
}

public class CustomerVendorListConversationsReponseDTO
{
    public ulong Id { get; set; }
    
    public UserResponseDTO Vendor { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
}