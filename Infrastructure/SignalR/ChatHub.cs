using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.SignalR;

/// <summary>
/// SignalR Hub để xử lý chat real-time giữa Customer và Vendor
/// </summary>
[Authorize]
public class ChatHub : BaseHub
{
    /// <summary>
    /// Khi client kết nối tới Hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            // Add user vào group riêng của họ
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Khi client ngắt kết nối
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId.Value}");
            
            var role = GetCurrentUserRole();
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{role}");
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    

    /// <summary>
    /// Test connection - client có thể gọi để kiểm tra kết nối
    /// </summary>
    public async Task<string> Ping()
    {
        var userId = TryGetCurrentUserId();
        var role = GetCurrentUserRole();
        return $"Chat Hub - Pong from User {userId} (Role: {role})";
    }
}
