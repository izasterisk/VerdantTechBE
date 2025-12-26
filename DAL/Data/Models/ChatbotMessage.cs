using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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
    //public Dictionary<string, object> MessageText { get; set; } = new Dictionary<string, object>();
    public string MessageText { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ChatbotConversation Conversation { get; set; } = null!;
}
