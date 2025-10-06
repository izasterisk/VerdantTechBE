using System.ComponentModel.DataAnnotations;
using DAL.Data;

namespace BLL.DTO.Order;
public class OrderPreviewCreateDTO
{
    // [Required(ErrorMessage = "CustomerId là bắt buộc.")]
    // public ulong CustomerId { get; set; }

    // public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // [Required(ErrorMessage = "Tổng phụ (Subtotal) là bắt buộc.")]
    // [Range(0, double.MaxValue, ErrorMessage = "Tổng phụ không được âm.")]
    // public decimal Subtotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thuế không được âm.")]
    public decimal TaxAmount { get; set; } = 0.00m;

    // [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển không được âm.")]
    // public decimal ShippingFee { get; set; } = 0.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    public decimal DiscountAmount { get; set; } = 0.00m;

    // [Required(ErrorMessage = "Tổng tiền đơn hàng là bắt buộc.")]
    // [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0.")]
    // public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "AddressId là bắt buộc.")]
    public ulong AddressId { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [EnumDataType(typeof(OrderPaymentMethod), ErrorMessage = "Phương thức thanh toán không hợp lệ. Chỉ chấp nhận: Banking, COD, Installment.")]
    public OrderPaymentMethod OrderPaymentMethod { get; set; }

    // [MaxLength(100, ErrorMessage = "Phương thức vận chuyển không được vượt quá 100 ký tự.")]
    // public string? ShippingMethod { get; set; }
    
    // [MaxLength(100, ErrorMessage = "Mã theo dõi không được vượt quá 100 ký tự.")]
    // public string? TrackingNumber { get; set; }

    [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Notes { get; set; }

    // [MaxLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    // public string? CancelledReason { get; set; }

    // public DateTime? CancelledAt { get; set; }

    // public DateTime? ConfirmedAt { get; set; }

    // public DateTime? ShippedAt { get; set; }

    // public DateTime? DeliveredAt { get; set; }
    
    [Required(ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    public List<OrderDetailPreviewCreateDTO> OrderDetails { get; set; } = new();
}