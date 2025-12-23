using System.ComponentModel.DataAnnotations;
using DAL.Data;
using Microsoft.AspNetCore.Http;

namespace BLL.DTO.CustomerVendorConversation;

public class CustomerVendorMessageCreateDTO
{
    [Required(ErrorMessage = "CustomerId là bắt buộc")]
    [Range(1, ulong.MaxValue, ErrorMessage = "CustomerId phải lớn hơn hoặc bằng 1")]
    public ulong CustomerId { get; set; }

    [Required(ErrorMessage = "VendorId là bắt buộc")]
    [Range(1, ulong.MaxValue, ErrorMessage = "VendorId phải lớn hơn hoặc bằng 1")]
    public ulong VendorId { get; set; }
    
    [Range(1, ulong.MaxValue, ErrorMessage = "ProductId phải lớn hơn hoặc bằng 1")]
    public ulong? ProductId { get; set; }
    
    [Required(ErrorMessage = "Nội dung tin nhắn là bắt buộc")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Nội dung tin nhắn phải từ 1 đến 5000 ký tự")]
    public string MessageText { get; set; } = null!;
    
    public List<IFormFile>? Images { get; set; }
}