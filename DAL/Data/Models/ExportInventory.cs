using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Export inventory tracking (stock out) - hỗ trợ cả máy móc (có serial) và phân bón (có lot number)
/// </summary>
public partial class ExportInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    public ulong? ProductSerialId { get; set; }

    [Required]
    [StringLength(100)]
    public string LotNumber { get; set; } = null!;

    public ulong? OrderDetailId { get; set; }

    public int Quantity { get; set; } = 1;

    public int RefundQuantity { get; set; } = 0;

    public MovementType MovementType { get; set; } = MovementType.Sale;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    public ulong CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual ProductSerial? ProductSerial { get; set; }
    public virtual OrderDetail? OrderDetail { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
}

