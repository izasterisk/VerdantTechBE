using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Request messages table for storing conversations between user and admin/staff
/// </summary>
public partial class RequestMessage
{
    public ulong Id { get; set; }

    public ulong RequestId { get; set; }

    /// <summary>
    /// Admin/staff who replied (NULL if message is from user)
    /// </summary>
    public ulong? StaffId { get; set; }

    [Required]
    public string Description { get; set; } = null!;

    public string? ReplyNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Request Request { get; set; } = null!;
    public virtual User? Staff { get; set; }
}
