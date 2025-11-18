using BLL.DTO;
using BLL.DTO.Notification;
using DAL.Data;

namespace BLL.Interfaces;

public interface INotificationService
{
    Task<NotificationResponseDTO> CreateAndSendNotificationAsync(ulong userId, string title, string message, 
        NotificationReferenceType? referenceType = null, ulong? referenceId = null, CancellationToken cancellationToken = default);
    Task CreateAndSendMultipleNotificationsAsync(List<ulong> userId, string title,
        string message, NotificationReferenceType referenceType, ulong referenceId,
        CancellationToken cancellationToken = default);
    Task<NotificationResponseDTO> RevertReadStatusAsync(ulong notificationId, CancellationToken cancellationToken = default);
    Task<PagedResponse<NotificationResponseDTO>> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(ulong notificationId, CancellationToken cancellationToken = default);
}