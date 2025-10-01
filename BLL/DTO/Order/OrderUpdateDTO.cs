using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Order;

public class OrderUpdateDTO
{
    // [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
    [EnumDataType(typeof(OrderStatus), ErrorMessage = "Trạng thái phải là Pending, Confirmed, Processing, Shipped, Delivered, Cancelled hoặc Refunded")]
    public OrderStatus? Status { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thuế (TaxAmount) không được âm.")]
    [DataType(DataType.Currency)]
    public decimal? TaxAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    [DataType(DataType.Currency)]
    public decimal? DiscountAmount { get; set; }

    // [Required(ErrorMessage = "AddressId là bắt buộc.")]
    [Range(1, ulong.MaxValue, ErrorMessage = "AddressId phải lớn hơn 0.")]
    public ulong? AddressId { get; set; }

    [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    [StringLength(100, ErrorMessage = "Phương thức vận chuyển không được vượt quá 100 ký tự.")]
    public string? ShippingMethod { get; set; }

    [StringLength(100, ErrorMessage = "Mã vận đơn (TrackingNumber) không được vượt quá 100 ký tự.")]
    public string? TrackingNumber { get; set; }

    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    public string? CancelledReason { get; set; }

    // [Required(ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    public List<OrderDetailUpdateDTO>? OrderDetails { get; set; } = new();
}