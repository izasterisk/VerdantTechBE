using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Green agricultural equipment products
/// </summary>
public partial class Product
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public ulong CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public decimal Price { get; set; }

    public decimal DiscountPercentage { get; set; } = 0.00m;

    /// <summary>
    /// Array of eco certifications (JSON)
    /// </summary>
    public List<string> GreenCertifications { get; set; } = new();

    [StringLength(10)]
    public string? EnergyEfficiencyRating { get; set; }

    /// <summary>
    /// Technical specifications as key-value pairs (JSON)
    /// </summary>
    public Dictionary<string, object> Specifications { get; set; } = new();

    /// <summary>
    /// Array of manual/guide URLs (JSON)
    /// </summary>
    public List<string> ManualUrls { get; set; } = new();

    /// <summary>
    /// Array of image URLs (JSON)
    /// </summary>
    public List<string> Images { get; set; } = new();

    public int WarrantyMonths { get; set; } = 12;

    public int StockQuantity { get; set; } = 0;

    public decimal? WeightKg { get; set; }

    /// <summary>
    /// {length, width, height} (JSON)
    /// </summary>
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

    public bool IsFeatured { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public long ViewCount { get; set; } = 0L;

    public long SoldCount { get; set; } = 0L;

    public decimal RatingAverage { get; set; } = 0.00m;

    public int TotalReviews { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User Vendor { get; set; } = null!;
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
