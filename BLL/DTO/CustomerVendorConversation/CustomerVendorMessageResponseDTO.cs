using System.ComponentModel.DataAnnotations;
using BLL.DTO.MediaLink;
using DAL.Data;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorMessageResponseDTO
{
    public ulong Id { get; set; }

    // public ulong ConversationId { get; set; }

    public CustomerVendorSenderType SenderType { get; set; }

    public string MessageText { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    
    public List<MediaLinkItemDTO> Images { get; set; } = new();
}