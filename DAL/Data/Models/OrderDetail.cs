using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Order line items
/// </summary>
public partial class OrderDetail
{
    public ulong Id { get; set; }

    public ulong OrderId { get; set; }

    public ulong ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; } = 0.00m;

    public decimal Subtotal { get; set; }

    public bool IsWalletCredited { get; set; } = false;

    public bool IsRefunded { get; set; } = false;

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<ExportInventory> ExportInventories { get; set; } = new List<ExportInventory>();
}
