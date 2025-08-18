using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Inventory movement tracking
/// </summary>
public partial class InventoryLog
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public InventoryType Type { get; set; }

    public int Quantity { get; set; }

    public int BalanceAfter { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    /// <summary>
    /// order, return, manual
    /// </summary>
    [StringLength(50)]
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual User? CreatedByNavigation { get; set; }
}
