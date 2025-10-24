using System.ComponentModel.DataAnnotations;
using BLL.DTO.Address;
using BLL.DTO.Courier;
using DAL.Data;

namespace BLL.DTO.Order;

public class OrderPreviewResponseDTO
{
    public Guid OrderPreviewId { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "CustomerId là bắt buộc.")]
    public ulong CustomerId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required(ErrorMessage = "Tổng phụ (Subtotal) là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Tổng phụ không được âm.")]
    public decimal Subtotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thuế không được âm.")]
    public decimal TaxAmount { get; set; } = 0.00m;

    // [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển không được âm.")]
    // public decimal ShippingFee { get; set; } = 0.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    public decimal DiscountAmount { get; set; } = 0.00m;

    [Required(ErrorMessage = "Tổng tiền đơn hàng là bắt buộc.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0.")]
    public decimal TotalAmountBeforeShippingFee { get; set; }
    
    // [Required(ErrorMessage = "AddressId là bắt buộc.")]
    // public ulong AddressId { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [EnumDataType(typeof(OrderPaymentMethod), ErrorMessage = "Phương thức thanh toán không hợp lệ. Chỉ chấp nhận: Banking, COD, Rent.")]
    public OrderPaymentMethod OrderPaymentMethod { get; set; }

    // [MaxLength(100, ErrorMessage = "Phương thức vận chuyển không được vượt quá 100 ký tự.")]
    // public string? ShippingMethod { get; set; }
    
    // [MaxLength(100, ErrorMessage = "Mã theo dõi không được vượt quá 100 ký tự.")]
    // public string? TrackingNumber { get; set; }

    [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Notes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Weight { get; set; }

    // [MaxLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    // public string? CancelledReason { get; set; }

    // public DateTime? CancelledAt { get; set; }

    // public DateTime? ConfirmedAt { get; set; }

    // public DateTime? ShippedAt { get; set; }

    // public DateTime? DeliveredAt { get; set; }
    
    [Required(ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm.")]
    public List<OrderDetailsPreviewResponseDTO> OrderDetails { get; set; } = new();
    public AddressResponseDTO Address { get; set; } = null!;
    public List<ShippingDetailDTO> ShippingDetails { get; set; } = new();
}

public class ShippingDetailDTO
{
    public int PriceTableId { get; set; }
    public string CarrierName { get; set; } = string.Empty;
    public string CarrierShortName { get; set; } = string.Empty;
    public string? CarrierLogo { get; set; }
    public string Service { get; set; } = string.Empty;
    public string ExpectedTxt { get; set; } = string.Empty;
    public decimal TotalFee { get; set; }
    public decimal TotalAmount { get; set; }
    public RateReportDTO? Report { get; set; }
}