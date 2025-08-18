using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Inventory movement tracking
/// </summary>
public partial class InventoryLog
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public string Type { get; set; } = null!;

    public int Quantity { get; set; }

    public int BalanceAfter { get; set; }

    public string? Reason { get; set; }

    /// <summary>
    /// order, return, manual
    /// </summary>
    public string? ReferenceType { get; set; }

    public ulong? ReferenceId { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;
}
