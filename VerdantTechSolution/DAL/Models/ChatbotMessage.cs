using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Individual chatbot messages
/// </summary>
public partial class ChatbotMessage
{
    public ulong Id { get; set; }

    public ulong ConversationId { get; set; }

    public string MessageType { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    /// <summary>
    /// Detected user intent
    /// </summary>
    public string? Intent { get; set; }

    /// <summary>
    /// Extracted entities from message
    /// </summary>
    public string? Entities { get; set; }

    /// <summary>
    /// AI confidence score
    /// </summary>
    public decimal? ConfidenceScore { get; set; }

    /// <summary>
    /// Quick reply suggestions
    /// </summary>
    public string? SuggestedActions { get; set; }

    /// <summary>
    /// Image or file attachments
    /// </summary>
    public string? Attachments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
