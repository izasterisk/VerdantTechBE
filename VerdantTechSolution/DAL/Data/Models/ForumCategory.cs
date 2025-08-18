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

    [StringLength(255)]
    public string? NameEn { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
}
