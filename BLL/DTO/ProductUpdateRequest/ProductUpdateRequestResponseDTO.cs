using BLL.DTO.Product;
using BLL.DTO.ProductCategory;
using BLL.DTO.User;
using BLL.DTO.MediaLink;
using DAL.Data;
using DAL.Data.Models;

namespace BLL.DTO.ProductUpdateRequest;

public class ProductUpdateRequestResponseDTO
{
    public ulong Id { get; set; }

    // 1. LINK TO ORIGINAL PRODUCT
    public ulong ProductId { get; set; }

    // 2. PRODUCT INFORMATION (Snapshot of proposed changes)
    public ulong CategoryId { get; set; }

    public ulong VendorId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionRate { get; set; }

    public decimal DiscountPercentage { get; set; }

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
    public string? ManualUrls { get; set; }

    /// <summary>
    /// Public URL for manual document access
    /// </summary>
    public string? PublicUrl { get; set; }

    public int WarrantyMonths { get; set; }

    public decimal WeightKg { get; set; }

    /// <summary>
    /// {length, width, height} (JSON)
    /// </summary>
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

    // 3. REQUEST MANAGEMENT FIELDS
    public ProductRegistrationStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public ulong? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public ProductResponseDTO? Product { get; set; }
    public ProductCategoryResponseDTO? Category { get; set; }
    public UserResponseDTO? Vendor { get; set; }
    public UserResponseDTO? ProcessedByUser { get; set; }
    public List<MediaLinkItemDTO> ProductImages { get; set; } = new();
}