using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Green agricultural equipment products
/// </summary>
public partial class Product
{
    public ulong Id { get; set; }

    public ulong CategoryId { get; set; }

    public ulong VendorId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionRate { get; set; } = 0.00m;

    public decimal DiscountPercentage { get; set; } = 0.00m;

    /// <summary>
    /// Energy efficiency rating (0-5, with 0 being lowest and 5 being highest)
    /// </summary>
    public int? EnergyEfficiencyRating { get; set; }

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
    /// Public URL for manual document access (v8.1)
    /// </summary>
    [StringLength(500)]
    public string? PublicUrl { get; set; }

    public int WarrantyMonths { get; set; } = 12;

    public int StockQuantity { get; set; } = 0;

    [Required]
    public decimal WeightKg { get; set; }

    /// <summary>
    /// {length, width, height} (JSON)
    /// </summary>
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public bool ForRent { get; set; } = false;

    public long ViewCount { get; set; } = 0L;

    public long SoldCount { get; set; } = 0L;

    public decimal RatingAverage { get; set; } = 0.00m;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual User Vendor { get; set; } = null!;
    public virtual ICollection<BatchInventory> BatchInventories { get; set; } = new List<BatchInventory>();
    public virtual ICollection<ExportInventory> ExportInventories { get; set; } = new List<ExportInventory>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<ProductCertificate> ProductCertificates { get; set; } = new List<ProductCertificate>();
}
