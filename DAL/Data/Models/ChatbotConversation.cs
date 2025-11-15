using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Chatbot conversation sessions
/// </summary>
public partial class ChatbotConversation
{
    public ulong Id { get; set; }

    public ulong CustomerId { get; set; }

    [StringLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// Conversation context and metadata
    /// </summary>
    public string? Context { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime StartedAt { get; set; }

    // Navigation Properties
    public virtual User Customer { get; set; } = null!;
    public virtual ICollection<ChatbotMessage> ChatbotMessages { get; set; } = new List<ChatbotMessage>();
}
