using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Blog articles and educational content
/// </summary>
public partial class BlogPost
{
    public ulong Id { get; set; }

    public ulong AuthorId { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string? Excerpt { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [StringLength(500)]
    public string? FeaturedImageUrl { get; set; }

    /// <summary>
    /// Array of tags (JSON)
    /// </summary>
    public List<string> Tags { get; set; } = new();

    [StringLength(255)]
    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    /// <summary>
    /// SEO keywords (JSON)
    /// </summary>
    public List<string> SeoKeywords { get; set; } = new();

    public long ViewCount { get; set; } = 0L;

    public int CommentCount { get; set; } = 0;

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public int? ReadingTimeMinutes { get; set; }

    public bool IsFeatured { get; set; } = false;

    public BlogStatus Status { get; set; } = BlogStatus.Draft;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Author { get; set; } = null!;
    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
}
