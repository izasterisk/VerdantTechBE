using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Forum post comments
/// </summary>
public partial class ForumComment
{
    public ulong Id { get; set; }

    public ulong PostId { get; set; }

    public ulong UserId { get; set; }

    /// <summary>
    /// For nested comments
    /// </summary>
    public ulong? ParentId { get; set; }

    public string Content { get; set; } = null!;

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public bool? IsSolution { get; set; }

    public string? Status { get; set; }

    public string? ModeratedReason { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ForumComment> InverseParent { get; set; } = new List<ForumComment>();

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual ForumComment? Parent { get; set; }

    public virtual ForumPost Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
