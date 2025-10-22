using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderCreateDTO
{
    [Required(ErrorMessage = "Mã dịch vụ vận chuyển là bắt buộc.")]
    public string CourierId { get; set; } = string.Empty;
}