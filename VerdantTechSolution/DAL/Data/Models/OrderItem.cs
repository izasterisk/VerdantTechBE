using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

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
    [Required]
    [StringLength(255)]
    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Snapshot of SKU
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ProductSku { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; } = 0.00m;

    public decimal Subtotal { get; set; }

    /// <summary>
    /// Snapshot of product specs (JSON)
    /// </summary>
    public Dictionary<string, object> Specifications { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
