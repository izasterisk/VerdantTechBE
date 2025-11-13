using AutoMapper;
using BLL.DTO;
using BLL.DTO.Notification;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;
    
    public NotificationService(IMapper mapper, INotificationRepository notificationRepository)
    {
        _mapper = mapper;
        _notificationRepository = notificationRepository;
    }
    
    public async Task<NotificationResponseDTO> RevertReadStatusAsync(ulong notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId, cancellationToken);
        if(notification.IsRead == true)
        {
            notification.IsRead = false;
        }
        else
        {
            notification.IsRead = true;
        }
        var updatedNotification = await _notificationRepository.UpdateNotificationAsync(notification, cancellationToken);
        return _mapper.Map<NotificationResponseDTO>(updatedNotification);
    }
    
    public async Task<PagedResponse<NotificationResponseDTO>> GetAllNotificationsByUserIdAsync(ulong userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var (notifications, totalCount) = await _notificationRepository.GetAllNotificationsByUserIdAsync(userId, page, pageSize, cancellationToken);
        var notificationDtos = _mapper.Map<List<NotificationResponseDTO>>(notifications);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedResponse<NotificationResponseDTO>
        {
            Data = notificationDtos,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalCount,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
    
    public async Task<bool> DeleteNotificationAsync(ulong notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId, cancellationToken);
        return await _notificationRepository.DeleteNotificationAsync(notification, cancellationToken);
    }
}