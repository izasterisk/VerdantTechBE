using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorConversationCreateDTO
{
    [Required(ErrorMessage = "VendorId là bắt buộc")]
    [Range(1, ulong.MaxValue, ErrorMessage = "VendorId phải lớn hơn hoặc bằng 1")]
    public ulong VendorId { get; set; }
    
    [Required(ErrorMessage = "Tin nhắn khởi tạo là bắt buộc")]
    public CustomerVendorMessageCreateDTO InitialMessage { get; set; } = null!;
}