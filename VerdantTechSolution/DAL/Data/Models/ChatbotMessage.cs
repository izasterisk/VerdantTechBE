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
    /// Detected user intent
    /// </summary>
    [StringLength(100)]
    public string? Intent { get; set; }

    /// <summary>
    /// Extracted entities from message (JSON)
    /// </summary>
    public Dictionary<string, object> Entities { get; set; } = new();

    /// <summary>
    /// AI confidence score
    /// </summary>
    public decimal? ConfidenceScore { get; set; }

    /// <summary>
    /// Quick reply suggestions (JSON)
    /// </summary>
    public List<string> SuggestedActions { get; set; } = new();

    /// <summary>
    /// Image or file attachments (JSON)
    /// </summary>
    public List<string> Attachments { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
