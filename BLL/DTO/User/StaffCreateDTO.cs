using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.User;

public class StaffCreateDTO
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 255 ký tự")]
    public string FullName { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [RegularExpression(@"^(\+84|84|0)?[3-9][0-9]{8}$", 
        ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
    public string? PhoneNumber { get; set; }
}