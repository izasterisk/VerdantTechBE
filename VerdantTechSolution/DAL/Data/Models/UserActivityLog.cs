using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// User activity tracking for analytics
/// </summary>
public partial class UserActivityLog
{
    public ulong Id { get; set; }

    public ulong UserId { get; set; }

    /// <summary>
    /// login, view_product, search, etc.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = null!;

    /// <summary>
    /// Activity details (JSON)
    /// </summary>
    public Dictionary<string, object> ActivityDetails { get; set; } = new();

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    [StringLength(255)]
    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
