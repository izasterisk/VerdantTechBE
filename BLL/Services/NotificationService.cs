using AutoMapper;
using BLL.DTO;
using BLL.DTO.Notification;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class NotificationService : INotificationService
{
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHub _notificationHub;
    
    public NotificationService(
        IMapper mapper, 
        INotificationRepository notificationRepository,
        INotificationHub notificationHub)
    {
        _mapper = mapper;
        _notificationRepository = notificationRepository;
        _notificationHub = notificationHub;
    }
    
    public async Task<NotificationResponseDTO> CreateAndSendNotificationAsync(
        ulong userId, 
        string title, 
        string message, 
        NotificationReferenceType? referenceType = null,
        ulong? referenceId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            IsRead = false
        };
        
        var createdNotification = await _notificationRepository.CreateNotificationAsync(notification, cancellationToken);
        var notificationDto = _mapper.Map<NotificationResponseDTO>(createdNotification);
        
        try
        {
            await _notificationHub.SendNotificationToUser(userId, notificationDto);
        }
        catch
        {
            // Không throw exception - notification đã lưu DB thành công
        }
        
        return notificationDto;
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