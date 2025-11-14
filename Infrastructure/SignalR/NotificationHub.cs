using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.SignalR;

/// <summary>
/// SignalR Hub để xử lý thông báo real-time
/// Đây là Infrastructure component - External Communication Mechanism
/// </summary>
[Authorize]
public class NotificationHub : BaseHub
{
    /// <summary>
    /// Khi client kết nối tới Hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = TryGetCurrentUserId();
        
        if (userId.HasValue)
        {
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
    /// Method để client có thể gọi để đánh dấu đã đọc
    /// </summary>
    public async Task MarkNotificationAsRead(ulong notificationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await Clients.Caller.SendCoreAsync("NotificationMarkedAsRead", new object[] { notificationId });
        }
        catch (UnauthorizedAccessException)
        {
            await Clients.Caller.SendCoreAsync("Error", new object[] { "Unauthorized" });
        }
    }
    
    /// <summary>
    /// Test connection - client có thể gọi để kiểm tra kết nối
    /// </summary>
    public async Task<string> Ping()
    {
        var userId = TryGetCurrentUserId();
        var role = GetCurrentUserRole();
        return $"Pong from User {userId} (Role: {role})";
    }
}
