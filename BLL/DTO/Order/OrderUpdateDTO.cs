using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Order;

public class OrderUpdateDTO
{
    [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    public string? CancelledReason { get; set; }
}