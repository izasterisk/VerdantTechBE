using System.ComponentModel.DataAnnotations;
using BLL.DTO.Address;
using BLL.DTO.User;
using DAL.Data;

namespace BLL.DTO.Order;

public class OrderResponseDTO
{
    [Required(ErrorMessage = "Mã đơn hàng là bắt buộc.")]
    public ulong Id { get; set; }

    // [Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
    // public ulong CustomerId { get; set; }
    public UserResponseDTO Customer { get; set; } = null!;

    [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc.")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required(ErrorMessage = "Tổng tiền hàng là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền hàng không được âm.")]
    public decimal Subtotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Thuế không được âm.")]
    public decimal TaxAmount { get; set; } = 0.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển không được âm.")]
    public decimal ShippingFee { get; set; } = 0.00m;

    [Range(0, double.MaxValue, ErrorMessage = "Giảm giá không được âm.")]
    public decimal DiscountAmount { get; set; } = 0.00m;

    [Required(ErrorMessage = "Tổng thanh toán là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Tổng thanh toán không được âm.")]
    public decimal TotalAmount { get; set; }

    // [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc.")]
    // public ulong AddressId { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [EnumDataType(typeof(OrderPaymentMethod), ErrorMessage = "Phương thức thanh toán không hợp lệ. Chỉ chấp nhận: Banking, COD, Rent.")]
    public OrderPaymentMethod OrderPaymentMethod { get; set; }

    [StringLength(100, ErrorMessage = "Phương thức giao hàng không được vượt quá 100 ký tự.")]
    public string? ShippingMethod { get; set; }

    [StringLength(100, ErrorMessage = "Mã vận đơn không được vượt quá 100 ký tự.")]
    public string? TrackingNumber { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? Notes { get; set; }
    
    public int Width { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Weight { get; set; }

    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    public string? CancelledReason { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? CancelledAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ConfirmedAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ShippedAt { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DeliveredAt { get; set; }

    [Required(ErrorMessage = "Ngày tạo đơn hàng là bắt buộc.")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [Required(ErrorMessage = "Ngày cập nhật đơn hàng là bắt buộc.")]
    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }

    public List<OrderDetailsResponseDTO>? OrderDetails { get; set; }
}

public class ProductResponseDTO
{
    public ulong Id { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public List<ProductImageResponseDTO> Images { get; set; } = new();
    public decimal UnitPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public Dictionary<string, object> Specifications { get; set; } = new();
    public Dictionary<string, decimal> DimensionsCm { get; set; } = new();
}

public class ProductImageResponseDTO
{
    public ulong Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public int SortOrder { get; set; }
}