using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Generic requests table for various request types
/// </summary>
public partial class Request
{
    public ulong Id { get; set; }

    public ulong RequesterId { get; set; }

    public RequestType RequestType { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public RequestPriority? Priority { get; set; } = null;

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public decimal? Amount { get; set; }

    public string? AdminNotes { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public ulong? AssignedTo { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Requester { get; set; } = null!;
    public virtual User? AssignedToNavigation { get; set; }
    public virtual User? ProcessedByNavigation { get; set; }
}
