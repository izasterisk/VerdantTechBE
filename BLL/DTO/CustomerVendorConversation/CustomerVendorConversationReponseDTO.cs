namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorConversationReponseDTO
{
    // public ulong Id { get; set; }

    // public ulong CustomerId { get; set; }

    // public ulong VendorId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
    
    public List<CustomerVendorMessageResponseDTO> CustomerVendorMessages { get; set; } = new();
}