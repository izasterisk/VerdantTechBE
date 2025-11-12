using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderDetailsResponseDTO
{
    [Required(ErrorMessage = "Mã chi tiết đơn hàng là bắt buộc.")]
    public ulong Id { get; set; }

    // [Required(ErrorMessage = "Mã đơn hàng là bắt buộc.")]
    // public ulong OrderId { get; set; }

    [Required(ErrorMessage = "Sản phẩm là bắt buộc.")]
    public ProductResponseDTO Product { get; set; } = null!;

    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 1.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm.")]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    public decimal DiscountAmount { get; set; } = 0.00m;

    [Required(ErrorMessage = "Thành tiền là bắt buộc.")]
    [Range(0, double.MaxValue, ErrorMessage = "Thành tiền không được âm.")]
    public decimal Subtotal { get; set; }

    // public bool IsWalletCredited { get; set; } = false;

    [Required(ErrorMessage = "Ngày cập nhật là bắt buộc.")]
    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }
}