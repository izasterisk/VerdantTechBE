using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Quản lý số seri từng sản phẩm trong lô hàng
/// </summary>
public partial class ProductSerial
{
    public ulong Id { get; set; }

    public ulong BatchInventoryId { get; set; }

    public ulong ProductId { get; set; }

    [Required]
    [StringLength(255)]
    public string SerialNumber { get; set; } = null!;

    public ProductSerialStatus Status { get; set; } = ProductSerialStatus.Stock;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual BatchInventory BatchInventory { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ExportInventory? ExportInventory { get; set; }
}
