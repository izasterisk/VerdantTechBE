using System;
using System.Collections.Generic;

namespace DAL.Models;

/// <summary>
/// Order line items
/// </summary>
public partial class OrderItem
{
    public ulong Id { get; set; }

    public ulong OrderId { get; set; }

    public ulong ProductId { get; set; }

    /// <summary>
    /// Snapshot of product name
    /// </summary>
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Snapshot of SKU
    /// </summary>
    public string ProductSku { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal Subtotal { get; set; }

    /// <summary>
    /// Snapshot of product specs
    /// </summary>
    public string? Specifications { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
