using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;
public class OrderDetailsShippingDTO
{
    [Required(ErrorMessage = "Mã sản phẩm là bắt buộc.")]
    [Range(1, ulong.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 1.")]
    public ulong ProductId { get; set; }
    
    [StringLength(50, ErrorMessage = "Số sê-ri không được vượt quá 50 ký tự.")]
    public string? SerialNumber { get; set; }
    
    [Required(ErrorMessage = "Số lô là bắt buộc")]
    [StringLength(50, ErrorMessage = "Số lô không được vượt quá 50 ký tự.")]
    public string LotNumber { get; set; } = null!;
}