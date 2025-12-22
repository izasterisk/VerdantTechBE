using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Messages in customer-vendor conversation
/// </summary>
public partial class CustomerVendorMessage
{
    public ulong Id { get; set; }

    public ulong ConversationId { get; set; }

    public CustomerVendorSenderType SenderType { get; set; }

    [Required]
    public string MessageText { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual CustomerVendorConversation Conversation { get; set; } = null!;
}
