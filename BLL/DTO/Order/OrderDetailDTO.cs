using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderDetailDTO
{
    [Required(ErrorMessage = "ProductId là bắt buộc.")]
    public ulong ProductId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 1.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    public decimal DiscountAmount { get; set; } = 0.00m;
}

public class OrderDetailUpdateDTO
{
    [Required(ErrorMessage = "OrderDetailId là bắt buộc.")]
    public ulong Id { get; set; }
    
    // [Required(ErrorMessage = "ProductId là bắt buộc.")]
    // public ulong ProductId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải ít nhất là 0.")]
    public int? Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
    public decimal? UnitPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm giá không được âm.")]
    public decimal? DiscountAmount { get; set; } = 0.00m;
}

public class OrderDetailResponseDTO
{
    public ulong Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public ProductResponseDTO Product { get; set; } = null!;
}