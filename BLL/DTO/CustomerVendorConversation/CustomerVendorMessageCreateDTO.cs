using System.ComponentModel.DataAnnotations;
using DAL.Data;
using Microsoft.AspNetCore.Http;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorMessageCreateDTO
{
    [Required(ErrorMessage = "Loại người gửi là bắt buộc")]
    [EnumDataType(typeof(CustomerVendorSenderType), ErrorMessage = "Loại người gửi phải là Customer hoặc Vendor")]
    public CustomerVendorSenderType SenderType { get; set; }
    
    [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Nội dung tin nhắn phải từ 1 đến 5000 ký tự")]
    public string MessageText { get; set; } = null!;
    
    public List<IFormFile>? Images { get; set; }
}