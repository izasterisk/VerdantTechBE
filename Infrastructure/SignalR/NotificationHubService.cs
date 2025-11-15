using BLL.Interfaces.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

/// <summary>
/// Service để gửi thông báo real-time qua SignalR
/// Đây là Infrastructure implementation
/// </summary>
public class NotificationHubService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gửi thông báo cho 1 user cụ thể
    /// </summary>
    public async Task SendNotificationToUser(ulong userId, object notification)
    {
        await _hubContext.Clients
            .Group($"User_{userId}")
            .SendCoreAsync("ReceiveNotification", [notification]);
    }

    /// <summary>
    /// Gửi thông báo cho nhiều user
    /// </summary>
    public async Task SendNotificationToMultipleUsers(List<ulong> userIds, object notification)
    {
        var groupNames = userIds.Select(id => $"User_{id}").ToList();
        
        await _hubContext.Clients
            .Groups(groupNames)
            .SendCoreAsync("ReceiveNotification", [notification]);
    }

    /// <summary>
    /// Gửi thông báo cho tất cả user đang online (broadcast)
    /// </summary>
    public async Task SendNotificationToAllUsers(object notification)
    {
        await _hubContext.Clients.All
            .SendCoreAsync("ReceiveNotification", [notification]);
    }

    /// <summary>
    /// Gửi thông báo cho tất cả user có role cụ thể
    /// </summary>
    public async Task SendNotificationToRole(string role, object notification)
    {
        await _hubContext.Clients
            .Group($"Role_{role}")
            .SendCoreAsync("ReceiveNotification", [notification]);
    }
}