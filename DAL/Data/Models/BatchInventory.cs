using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Theo dõi tồn kho nhập (hàng vào) - phiên bản đơn giản hóa
/// </summary>
public partial class BatchInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string Sku { get; set; } = null!;

    public ulong? VendorProfileId { get; set; }

    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [StringLength(100)]
    public string? LotNumber { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCostPrice { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? ManufacturingDate { get; set; }

    public QualityCheckStatus QualityCheckStatus { get; set; } = QualityCheckStatus.NotRequired;

    public ulong? QualityCheckedBy { get; set; }

    public DateTime? QualityCheckedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual VendorProfile? VendorProfile { get; set; }
    public virtual User? QualityCheckedByNavigation { get; set; }
}
