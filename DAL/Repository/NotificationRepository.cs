using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public NotificationRepository(IRepository<Notification> notificationRepository, VerdantTechDbContext dbContext)
    {
        _notificationRepository = notificationRepository;
        _dbContext = dbContext;
    }
    
    public async Task<Notification> CreateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.CreatedAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;
        notification.IsRead = false;
        return await _notificationRepository.CreateAsync(notification, cancellationToken);
    }

    public async Task CreateListNotificationsWithTransactionAsync(List<Notification> notifications, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var notification in notifications)
            {
                await CreateNotificationAsync(notification, cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Notification> UpdateNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.UpdatedAt = DateTime.UtcNow;
        return await _notificationRepository.UpdateAsync(notification, cancellationToken);
    }
    
    public async Task<bool> DeleteNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.DeleteAsync(notification, cancellationToken);
    }
    
    public async Task<Notification> GetNotificationByIdAsync(ulong notificationId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetAsync(n => n.Id == notificationId,
            useNoTracking: true, cancellationToken: cancellationToken) ??
               throw new KeyNotFoundException($"Không tồn tại thông báo với ID {notificationId}.");
    }
    
    public async Task<(List<Notification>, int totalCount)> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetPaginatedWithRelationsAsync(
            page,
            pageSize,
            n => n.UserId == userId,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(n => n.CreatedAt),
            cancellationToken: cancellationToken
        );
    }
}