using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product snapshot for tracking product history (both proposed changes and approved versions)
/// </summary>
public partial class ProductSnapshot
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

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
    /// Public URL for manual document access
    /// </summary>
    [StringLength(500)]
    public string? PublicUrl { get; set; }

    public int WarrantyMonths { get; set; } = 12;

    [Required]
    public decimal WeightKg { get; set; }

    /// <summary>
    /// Product dimensions: length, width, height in cm (JSON)
    /// </summary>
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

    public ulong? RegistrationId { get; set; }

    /// <summary>
    /// Snapshot type: Proposed (pending approval) or History (approved and applied to product)
    /// </summary>
    public ProductSnapshotType SnapshotType { get; set; } = ProductSnapshotType.Proposed;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual User Vendor { get; set; } = null!;
    public virtual ProductRegistration? Registration { get; set; }
    
    public virtual ICollection<ProductUpdateRequest> ProductUpdateRequests { get; set; } = new List<ProductUpdateRequest>();
}
