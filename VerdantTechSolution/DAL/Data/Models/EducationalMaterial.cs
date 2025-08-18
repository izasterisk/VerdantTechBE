using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Educational content created by experts and vendors
/// </summary>
public partial class EducationalMaterial
{
    public ulong Id { get; set; }

    public ulong CreatedBy { get; set; }

    public MaterialType MaterialType { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// URL to file or video
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ContentUrl { get; set; } = null!;

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public decimal? FileSizeMb { get; set; }

    /// <summary>
    /// For video content
    /// </summary>
    public int? DurationMinutes { get; set; }

    public Language Language { get; set; } = Language.Vi;

    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Beginner;

    /// <summary>
    /// Array of related topics (JSON)
    /// </summary>
    public List<string> Topics { get; set; } = new();

    /// <summary>
    /// Array of audience types (JSON)
    /// </summary>
    public List<string> TargetAudience { get; set; } = new();

    public long DownloadCount { get; set; } = 0L;

    public long ViewCount { get; set; } = 0L;

    public decimal RatingAverage { get; set; } = 0.00m;

    public int TotalRatings { get; set; } = 0;

    public bool IsPremium { get; set; } = false;

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User CreatedByNavigation { get; set; } = null!;
}
