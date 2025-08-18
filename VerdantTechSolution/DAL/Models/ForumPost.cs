using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Forum discussion posts
/// </summary>
public partial class ForumPost
{
    public ulong Id { get; set; }

    public ulong CategoryId { get; set; }

    public ulong UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    /// <summary>
    /// Array of tags
    /// </summary>
    public string? Tags { get; set; }

    public long? ViewCount { get; set; }

    public int? ReplyCount { get; set; }

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public bool? IsPinned { get; set; }

    public bool? IsLocked { get; set; }

    public string? Status { get; set; }

    public string? ModeratedReason { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ForumCategory Category { get; set; } = null!;

    public virtual ICollection<ForumComment> ForumComments { get; set; } = new List<ForumComment>();

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
