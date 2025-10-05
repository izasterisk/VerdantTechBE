using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Individual chatbot messages
/// </summary>
public partial class ChatbotMessage
{
    public ulong Id { get; set; }

    public ulong ConversationId { get; set; }

    public MessageType MessageType { get; set; }

    [Required]
    public string MessageText { get; set; } = null!;

    /// <summary>
    /// Image or file attachment URLs, comma-separated
    /// </summary>
    [StringLength(1000)]
    public string? Attachments { get; set; }

    [StringLength(500)]
    public string? PublicUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
