using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Order;

public class OrderCreateDTO
{
    public string ShippingDetailId { get; set; } = string.Empty;
}