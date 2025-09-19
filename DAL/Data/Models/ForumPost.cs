using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Forum discussion posts
/// </summary>
public partial class ForumPost
{
    public ulong Id { get; set; }

    public ulong ForumCategoryId { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    /// <summary>
    /// Mixed content blocks: [{"order": 1, "type": "text", "content": "Hello world"}, {"order": 2, "type": "image", "content": "https://example.com/image.jpg"}]
    /// </summary>
    [Required]
    public List<ContentBlock> Content { get; set; } = new();

    /// <summary>
    /// Tags, comma-separated list
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }

    public long ViewCount { get; set; } = 0L;

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public bool IsPinned { get; set; } = false;

    public ForumPostStatus Status { get; set; } = ForumPostStatus.Visible;


    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ForumCategory ForumCategory { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();
}
