using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Audit trail for important system changes
/// </summary>
public partial class AuditLog
{
    public ulong Id { get; set; }

    public ulong? UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = null!;

    public ulong EntityId { get; set; }

    /// <summary>
    /// Old values (JSON)
    /// </summary>
    public Dictionary<string, object>? OldValues { get; set; }

    /// <summary>
    /// New values (JSON)
    /// </summary>
    public Dictionary<string, object>? NewValues { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual User? User { get; set; }
}
