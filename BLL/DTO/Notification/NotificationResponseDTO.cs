using DAL.Data;
using DAL.Data.Models;

namespace BLL.DTO.Notification;

public class NotificationResponseDTO
{
    public ulong Id { get; set; }
    // public ulong UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationReferenceType? ReferenceType { get; set; }
    public ulong? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
