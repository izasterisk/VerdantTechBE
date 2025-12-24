using BLL.Interfaces.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

/// <summary>
/// Service để gửi tin nhắn real-time qua SignalR
/// </summary>
public class ChatHubService : IChatHub
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gửi tin nhắn cho 1 user cụ thể
    /// </summary>
    public async Task SendMessageToUser(ulong userId, object message)
    {
        await _hubContext.Clients
            .Group($"User_{userId}")
            .SendCoreAsync("ReceiveMessage", new object[] { message });
    }

    /// <summary>
    /// Gửi tin nhắn cho cả customer và vendor trong conversation
    /// </summary>
    public async Task SendMessageToConversation(ulong customerId, ulong vendorId, object message)
    {
        var groupNames = new[] { $"User_{customerId}", $"User_{vendorId}" };
        
        await _hubContext.Clients
            .Groups(groupNames)
            .SendCoreAsync("ReceiveMessage", new object[] { message });
    }


}
