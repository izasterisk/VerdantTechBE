using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Export inventory tracking (stock out) - phiên bản đơn giản hóa
/// </summary>
public partial class ExportInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong? OrderId { get; set; }

    public int Quantity { get; set; }

    public MovementType MovementType { get; set; } = MovementType.Sale;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    public ulong CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
}
