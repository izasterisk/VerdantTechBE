using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Product registration requests from vendors
/// </summary>
public partial class ProductRegistration
{
    public ulong Id { get; set; }

    public ulong VendorId { get; set; }

    public ulong CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProposedProductCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string ProposedProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    [StringLength(10)]
    public string? EnergyEfficiencyRating { get; set; }

    public string Specifications { get; set; } = "{}";

    [StringLength(1000)]
    public string? ManualUrls { get; set; }

    [StringLength(1000)]
    public string? Images { get; set; }

    public int WarrantyMonths { get; set; } = 12;

    public decimal? WeightKg { get; set; }

    public string DimensionsCm { get; set; } = "{}";

    public ProductRegistrationStatus Status { get; set; } = ProductRegistrationStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public ulong? ApprovedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    // Navigation Properties
    public virtual User Vendor { get; set; } = null!;
    public virtual ProductCategory Category { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
}

