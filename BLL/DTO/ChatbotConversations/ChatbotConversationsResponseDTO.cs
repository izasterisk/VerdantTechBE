namespace BLL.DTO.ChatbotConversations;

public class ChatbotConversationsResponseDTO
{
    public ulong Id { get; set; }

    // public ulong CustomerId { get; set; }

    public string? Title { get; set; }
    
    public string? Context { get; set; }
    
    public string SessionId { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime StartedAt { get; set; }
}

public class SendNewMessageResponseDTO
{
    public ChatbotConversationsResponseDTO Conversation { get; set; } = null!;
    public ChatbotMessagesResponseDTO Message { get; set; } = null!;
}