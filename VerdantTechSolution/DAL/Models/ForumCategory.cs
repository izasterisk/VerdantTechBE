using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Forum discussion categories
/// </summary>
public partial class ForumCategory
{
    public ulong Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
}
