using BLL.DTO;
using BLL.DTO.Notification;

namespace BLL.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDTO> RevertReadStatusAsync(ulong notificationId, CancellationToken cancellationToken = default);
    Task<PagedResponse<NotificationResponseDTO>> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(ulong notificationId, CancellationToken cancellationToken = default);
}