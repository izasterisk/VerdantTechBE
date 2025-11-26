using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Request;

public class RequestMessageCreateDTO
{
    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10 đến 2000 ký tự")]
    public string Description { get; set; } = null!;
    
    public List<RequestImageDTO>? Images { get; set; }
}