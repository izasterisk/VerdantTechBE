using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Blog post comments
/// </summary>
public partial class BlogComment
{
    public ulong Id { get; set; }

    public ulong PostId { get; set; }

    public ulong UserId { get; set; }

    public ulong? ParentId { get; set; }

    public string Content { get; set; } = null!;

    public int? LikeCount { get; set; }

    public int? DislikeCount { get; set; }

    public string? Status { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BlogComment> InverseParent { get; set; } = new List<BlogComment>();

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual BlogComment? Parent { get; set; }

    public virtual BlogPost Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
