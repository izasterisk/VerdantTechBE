using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace DAL.Data.Models;

/// <summary>
/// Theo dõi tồn kho nhập (hàng vào)
/// </summary>
public partial class PurchaseInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    [StringLength(100)]
    public string Sku { get; set; } = null!;

    public ulong? VendorProfileId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCostPrice { get; set; }

    public decimal TotalCost { get; set; }

    public decimal CommissionRate { get; set; } = 0.00m;

    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [StringLength(100)]
    public string? LotNumber { get; set; }

    public string? SerialNumbers { get; set; }

    [StringLength(255)]
    public string? SupplierInvoice { get; set; }

    [StringLength(100)]
    public string? PurchaseOrderNumber { get; set; }

    [StringLength(100)]
    public string? WarehouseLocation { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateOnly? ManufacturingDate { get; set; }

    public QualityCheckStatus QualityCheckStatus { get; set; } = QualityCheckStatus.NotRequired;

    [StringLength(500)]
    public string? QualityCheckNotes { get; set; }

    public ulong? QualityCheckedBy { get; set; }

    public DateTime? QualityCheckedAt { get; set; }

    public ConditionOnArrival ConditionOnArrival { get; set; } = ConditionOnArrival.New;

    [StringLength(500)]
    public string? DamageNotes { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int BalanceAfter { get; set; }

    public ulong CreatedBy { get; set; }

    public DateTime PurchasedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual VendorProfile? VendorProfile { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual User? QualityCheckedByNavigation { get; set; }
}
