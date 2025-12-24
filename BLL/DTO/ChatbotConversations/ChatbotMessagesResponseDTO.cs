using System.Collections.Generic;
using DAL.Data;

namespace BLL.DTO.ChatbotConversations;

public class ChatbotMessagesResponseDTO
{
    public ulong Id { get; set; }

    // public ulong ConversationId { get; set; }

    public MessageType MessageType { get; set; }

    public Dictionary<string, object> MessageText { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}