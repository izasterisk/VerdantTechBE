using DAL.Data.Models;

namespace DAL.IRepository;

public interface INotificationRepository
{
    Task<Notification> CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task CreateListNotificationsWithTransactionAsync(List<Notification> notifications, CancellationToken cancellationToken = default);
    Task<Notification> GetNotificationByIdAsync(ulong notificationId, CancellationToken cancellationToken = default);
    Task<(List<Notification>, int totalCount)> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Notification> UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}