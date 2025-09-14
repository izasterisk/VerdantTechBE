using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Sales inventory tracking (stock out)
/// </summary>
public partial class SalesInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong? OrderId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitSalePrice { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal CommissionAmount { get; set; } = 0.00m;

    public int BalanceAfter { get; set; }

    public MovementType MovementType { get; set; } = MovementType.Sale;

    [StringLength(500)]
    public string? Notes { get; set; }

    public ulong CreatedBy { get; set; }

    public DateTime SoldAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
}
