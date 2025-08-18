using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Educational content created by experts and vendors
/// </summary>
public partial class EducationalMaterial
{
    public ulong Id { get; set; }

    public ulong CreatedBy { get; set; }

    public string MaterialType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// URL to file or video
    /// </summary>
    public string ContentUrl { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public decimal? FileSizeMb { get; set; }

    /// <summary>
    /// For video content
    /// </summary>
    public int? DurationMinutes { get; set; }

    public string? Language { get; set; }

    public string? DifficultyLevel { get; set; }

    /// <summary>
    /// Array of related topics
    /// </summary>
    public string? Topics { get; set; }

    /// <summary>
    /// Array of audience types
    /// </summary>
    public string? TargetAudience { get; set; }

    public long? DownloadCount { get; set; }

    public long? ViewCount { get; set; }

    public decimal? RatingAverage { get; set; }

    public int? TotalRatings { get; set; }

    public bool? IsPremium { get; set; }

    public string? Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
}
