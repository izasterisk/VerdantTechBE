using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Hierarchical product categories
/// </summary>
public partial class ProductCategory
{
    public ulong Id { get; set; }

    public ulong? ParentId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool SerialRequired { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ProductCategory? Parent { get; set; }
    public virtual ICollection<ProductCategory> InverseParent { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
