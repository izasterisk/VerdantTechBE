using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

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

    [Required]
    public string Content { get; set; } = null!;

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public bool IsSolution { get; set; } = false;

    public ForumCommentStatus Status { get; set; } = ForumCommentStatus.Visible;

    public string? ModeratedReason { get; set; }

    public ulong? ModeratedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ForumPost Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ForumComment? Parent { get; set; }
    public virtual User? ModeratedByNavigation { get; set; }
    public virtual ICollection<ForumComment> InverseParent { get; set; } = new List<ForumComment>();
}
