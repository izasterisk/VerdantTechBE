using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product update requests management (workflow state only, actual data stored in ProductSnapshot)
/// </summary>
public partial class ProductUpdateRequest
{
    public ulong Id { get; set; }

    public ulong ProductSnapshotId { get; set; }

    public ulong ProductId { get; set; }

    public ProductRegistrationStatus Status { get; set; } = ProductRegistrationStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ProductSnapshot ProductSnapshot { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual User? ProcessedByUser { get; set; }
}
