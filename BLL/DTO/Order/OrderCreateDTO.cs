using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderCreateDTO
{
    [Required(ErrorMessage = "Mã dịch vụ vận chuyển là bắt buộc.")]
    public int PriceTableId { get; set; }
}