using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Green agricultural equipment products
/// </summary>
public partial class Product
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public ulong CategoryId { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountPercentage { get; set; }

    /// <summary>
    /// Array of eco certifications
    /// </summary>
    public string? GreenCertifications { get; set; }

    public string? EnergyEfficiencyRating { get; set; }

    /// <summary>
    /// Technical specifications as key-value pairs
    /// </summary>
    public string? Specifications { get; set; }

    /// <summary>
    /// Array of manual/guide URLs
    /// </summary>
    public string? ManualUrls { get; set; }

    /// <summary>
    /// Array of image URLs
    /// </summary>
    public string? Images { get; set; }

    public int? WarrantyMonths { get; set; }

    public int? StockQuantity { get; set; }

    public int? MinOrderQuantity { get; set; }

    public decimal? WeightKg { get; set; }

    /// <summary>
    /// {length, width, height}
    /// </summary>
    public string? DimensionsCm { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsActive { get; set; }

    public long? ViewCount { get; set; }

    public long? SoldCount { get; set; }

    public decimal? RatingAverage { get; set; }

    public int? TotalReviews { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual User Vendor { get; set; } = null!;
}
