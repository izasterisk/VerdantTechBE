using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Blog post comments
/// </summary>
public partial class BlogComment
{
    public ulong Id { get; set; }

    public ulong PostId { get; set; }

    public ulong UserId { get; set; }

    public ulong? ParentId { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    public int LikeCount { get; set; } = 0;

    public int DislikeCount { get; set; } = 0;

    public BlogCommentStatus Status { get; set; } = BlogCommentStatus.Pending;

    public ulong? ModeratedBy { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual BlogPost Post { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual BlogComment? Parent { get; set; }
    public virtual User? ModeratedByNavigation { get; set; }
    public virtual ICollection<BlogComment> InverseParent { get; set; } = new List<BlogComment>();
}
