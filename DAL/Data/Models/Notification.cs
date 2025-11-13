using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// User notifications for system events
/// </summary>
public partial class Notification
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public NotificationReferenceType? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
