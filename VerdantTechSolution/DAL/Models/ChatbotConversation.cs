using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Chatbot conversation sessions
/// </summary>
public partial class ChatbotConversation
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public string SessionId { get; set; } = null!;

    public string? Title { get; set; }

    /// <summary>
    /// Conversation context and metadata
    /// </summary>
    public string? Context { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual ICollection<ChatbotMessage> ChatbotMessages { get; set; } = new List<ChatbotMessage>();

    public virtual User User { get; set; } = null!;
}
