using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Forum discussion posts
/// </summary>
public partial class ForumPost
{
    public ulong Id { get; set; }

    public ulong CategoryId { get; set; }

    public ulong UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    /// <summary>
    /// Mixed content blocks: text, images, videos, etc.
    /// </summary>
    [Required]
    public List<ContentBlock> Content { get; set; } = new();

    /// <summary>
    /// Array of tags (JSON)
    /// </summary>
    public List<string> Tags { get; set; } = new();

    public long ViewCount { get; set; } = 0L;

    public int ReplyCount { get; set; } = 0;

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public bool IsPinned { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    public ForumPostStatus Status { get; set; } = ForumPostStatus.Published;

    public string? ModeratedReason { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime LastActivityAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ForumCategory Category { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User? ModeratedByNavigation { get; set; }
    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();
}
