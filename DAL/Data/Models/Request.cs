using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Generic requests table for various request types
/// </summary>
public partial class Request
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    public RequestType RequestType { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public string? ReplyNotes { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual User? ProcessedByNavigation { get; set; }
}
