using DAL.Data;

namespace BLL.DTO.ChatbotConversations;

public class ChatbotMessagesResponseDTO
{
    public ulong Id { get; set; }

    // public ulong ConversationId { get; set; }

    public MessageType MessageType { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}