using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Green agricultural equipment products
/// </summary>
public partial class Product
{
    public ulong Id { get; set; }

    public ulong CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? NameEn { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public decimal Price { get; set; }

    public decimal CostPrice { get; set; } = 0.00m;

    public decimal CommissionRate { get; set; } = 0.00m;

    public decimal DiscountPercentage { get; set; } = 0.00m;

    /// <summary>
    /// Eco certification codes, comma-separated
    /// </summary>
    [StringLength(500)]
    public string? GreenCertifications { get; set; }

    [StringLength(10)]
    public string? EnergyEfficiencyRating { get; set; }

    /// <summary>
    /// Technical specifications as key-value pairs (JSON)
    /// </summary>
    public Dictionary<string, object> Specifications { get; set; } = new();

    /// <summary>
    /// Manual/guide URLs, comma-separated
    /// </summary>
    [StringLength(1000)]
    public string? ManualUrls { get; set; }

    /// <summary>
    /// Image URLs, comma-separated
    /// </summary>
    [StringLength(1000)]
    public string? Images { get; set; }

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
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual ICollection<PurchaseInventory> PurchaseInventories { get; set; } = new List<PurchaseInventory>();
    public virtual ICollection<SalesInventory> SalesInventories { get; set; } = new List<SalesInventory>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
}
