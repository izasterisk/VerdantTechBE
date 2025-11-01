using System.ComponentModel.DataAnnotations;

namespace DAL.Data.Models;

/// <summary>
/// Export inventory tracking (stock out) - hỗ trợ cả máy móc (có serial) và phân bón (có lot number)
/// </summary>
public partial class ExportInventory
{
    public ulong Id { get; set; }

    public ulong ProductId { get; set; }

    /// <summary>
    /// ID số seri sản phẩm (dùng cho máy móc/thiết bị có serial number)
    /// NULL nếu sản phẩm không có serial (phân bón, vật tư)
    /// </summary>
    public ulong? ProductSerialId { get; set; }

    /// <summary>
    /// Số lô sản xuất (dùng cho phân bón, vật tư không có serial number)
    /// NULL nếu sản phẩm có serial number
    /// </summary>
    [StringLength(100)]
    public string? LotNumber { get; set; }

    public ulong? OrderId { get; set; }

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
    public virtual Order? Order { get; set; }
    public virtual User CreatedByNavigation { get; set; } = null!;
}

