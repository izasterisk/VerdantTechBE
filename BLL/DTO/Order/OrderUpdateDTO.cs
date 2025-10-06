using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Order;

public class OrderUpdateDTO
{
    // [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
    [EnumDataType(typeof(OrderStatus), ErrorMessage = "Trạng thái phải là Pending, Confirmed, Processing, Shipped, Delivered, Cancelled hoặc Refunded")]
    public OrderStatus? Status { get; set; }

    // [StringLength(100, ErrorMessage = "Mã vận đơn (TrackingNumber) không được vượt quá 100 ký tự.")]
    // public string? TrackingNumber { get; set; }
    
    [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    public string? CancelledReason { get; set; } 
    // public List<OrderDetailUpdateDTO>? OrderDetails { get; set; } = new();
}