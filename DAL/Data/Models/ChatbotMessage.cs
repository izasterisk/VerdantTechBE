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

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
