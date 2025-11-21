using System.ComponentModel.DataAnnotations;
using BLL.DTO.Order;
using BLL.DTO.User;
using DAL.Data;

namespace BLL.DTO.ExportInventory;

public class ExportInventoryResponseDTO
{
    [Range(1, ulong.MaxValue, ErrorMessage = "Id phải lớn hơn 0")]
    public ulong Id { get; set; }

    // [Range(1, ulong.MaxValue, ErrorMessage = "Mã sản phẩm phải lớn hơn 0")]
    // public ulong ProductId { get; set; }
    public ProductResponseDTO Product { get; set; } = null!;
    
    // [Range(1, ulong.MaxValue, ErrorMessage = "Số sê-ri sản phẩm phải lớn hơn 0")]
    // public ulong? ProductSerialId { get; set; }
    public string? ProductSerialNumber { get; set; }

    [StringLength(255, ErrorMessage = "Số lô không được vượt quá 255 ký tự")]
    public string? LotNumber { get; set; }

    // [Range(1, ulong.MaxValue, ErrorMessage = "Mã đơn hàng phải lớn hơn 0")]
    // public ulong? OrderId { get; set; }

    [Required(ErrorMessage = "MovementType là bắt buộc")]
    [EnumDataType(typeof(MovementType), ErrorMessage = "MovementType phải là một trong các giá trị hợp lệ: " +
                                                       "ReturnToVendor, Damage, Loss, Adjustment.")]
    public MovementType MovementType { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? Notes { get; set; }

    // [Range(1, ulong.MaxValue, ErrorMessage = "Người tạo phải lớn hơn 0")]
    // public ulong CreatedBy { get; set; }
    public UserResponseDTO CreatedBy { get; set; } = null!;
    
    [Required(ErrorMessage = "Thời gian tạo là bắt buộc")]
    public DateTime CreatedAt { get; set; }
    
    [Required(ErrorMessage = "Thời gian cập nhật là bắt buộc")]
    public DateTime UpdatedAt { get; set; }
}