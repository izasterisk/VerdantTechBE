using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Hierarchical product categories
/// </summary>
public partial class ProductCategory
{
    public ulong Id { get; set; }

    public ulong? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ProductCategory> InverseParent { get; set; } = new List<ProductCategory>();

    public virtual ProductCategory? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
