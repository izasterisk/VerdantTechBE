using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Blog articles and educational content
/// </summary>
public partial class BlogPost
{
    public ulong Id { get; set; }

    public ulong AuthorId { get; set; }

    public string Category { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Excerpt { get; set; }

    public string Content { get; set; } = null!;

    public string? FeaturedImageUrl { get; set; }

    /// <summary>
    /// Array of tags
    /// </summary>
    public string? Tags { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public long? ViewCount { get; set; }

    public int? CommentCount { get; set; }

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public int? ReadingTimeMinutes { get; set; }

    public bool? IsFeatured { get; set; }

    public string? Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
}
