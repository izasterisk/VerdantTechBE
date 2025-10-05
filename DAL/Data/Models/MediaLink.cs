using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Centralized media storage for multiple entity types (v8.1)
/// </summary>
public partial class MediaLink
{
    public ulong Id { get; set; }

    [Required]
    public MediaOwnerType OwnerType { get; set; }

    public ulong OwnerId { get; set; }

    [Required]
    [StringLength(1024)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(512)]
    public string? ImagePublicId { get; set; }

    public MediaPurpose Purpose { get; set; } = MediaPurpose.None;

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
