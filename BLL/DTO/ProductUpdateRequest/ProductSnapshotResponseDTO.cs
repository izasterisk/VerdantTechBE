using System.ComponentModel.DataAnnotations;
using BLL.DTO.MediaLink;
using DAL.Data;

namespace BLL.DTO.ProductUpdateRequest;

public class ProductSnapshotResponseDTO
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong CategoryId { get; set; }

    public ulong VendorId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal CommissionRate { get; set; } = 0.00m;

    public decimal DiscountPercentage { get; set; } = 0.00m;

    public int? EnergyEfficiencyRating { get; set; }

    public Dictionary<string, object> Specifications { get; set; } = new();

    public string? ManualUrls { get; set; }

    public string? PublicUrl { get; set; }

    public int WarrantyMonths { get; set; } = 12;

    public decimal WeightKg { get; set; }

    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();

    public ulong? RegistrationId { get; set; }

    public ProductSnapshotType SnapshotType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    public List<MediaLinkItemDTO> Images { get; set; } = new();
}