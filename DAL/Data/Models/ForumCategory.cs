using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Forum discussion categories
/// </summary>
public partial class ForumCategory
{
    public ulong Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
}