namespace BLL.Interfaces.Infrastructure;

/// <summary>
/// Interface cho ChatHub Service để gửi tin nhắn real-time
/// </summary>
public interface IChatHub
{
    /// <summary>
    /// Gửi tin nhắn cho một user cụ thể
    /// </summary>
    Task SendMessageToUser(ulong userId, object message);
    
    /// <summary>
    /// Gửi tin nhắn cho cả customer và vendor trong conversation
    /// </summary>
    Task SendMessageToConversation(ulong customerId, ulong vendorId, object message);
}
