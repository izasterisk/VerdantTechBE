namespace BLL.Interfaces.Infrastructure;

/// <summary>
/// Interface cho NotificationHub Service để gửi thông báo real-time
/// </summary>
public interface INotificationHub
{
    /// <summary>
    /// Gửi thông báo cho một user cụ thể
    /// </summary>
    Task SendNotificationToUser(ulong userId, object notification);
    
    /// <summary>
    /// Gửi thông báo cho nhiều user
    /// </summary>
    Task SendNotificationToMultipleUsers(List<ulong> userIds, object notification);
    
    /// <summary>
    /// Gửi thông báo cho tất cả user đang online (broadcast)
    /// </summary>
    Task SendNotificationToAllUsers(object notification);
    
    /// <summary>
    /// Gửi thông báo cho user theo role cụ thể (Staff, Admin, Vendor...)
    /// </summary>
    Task SendNotificationToRole(string role, object notification);
}
