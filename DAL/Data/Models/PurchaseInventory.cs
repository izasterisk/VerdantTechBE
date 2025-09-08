using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Purchase inventory tracking (stock in)
/// </summary>
public partial class PurchaseInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong? VendorProfileId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCostPrice { get; set; }

    public decimal TotalCost { get; set; }

    public decimal CommissionRate { get; set; } = 0.00m;

    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [StringLength(255)]
    public string? SupplierInvoice { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int BalanceAfter { get; set; }

    public ulong CreatedBy { get; set; }

    public DateTime PurchasedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual VendorProfile? VendorProfile { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
}
