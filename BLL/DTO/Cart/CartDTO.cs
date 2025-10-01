using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Cart;

public class CartDTO
{
    [Required(ErrorMessage = "ProductId là bắt buộc.")]
    [Range(1, ulong.MaxValue, ErrorMessage = "ProductId phải lớn hơn 0.")]
    public ulong ProductId { get; set; }

    [Required(ErrorMessage = "Quantity là bắt buộc.")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity phải lớn hơn hoặc bằng 0.")]
    public int Quantity { get; set; }
}