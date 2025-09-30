using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderCreateDTO
{
    // [Required(ErrorMessage = "CustomerId là bắt buộc.")]
    // public ulong CustomerId { get; set; }

    // [Range(0, double.MaxValue, ErrorMessage = "Tổng phụ (Subtotal) không được âm.")]
    // public decimal Subtotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thuế (TaxAmount) không được âm.")]
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

    [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    public List<OrderDetailDTO> OrderDetails { get; set; } = new();
}