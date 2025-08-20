using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Customer;

public class CustomerCreateDTO
{
    // public ulong Id { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự")]
    public string Password { get; set; } = null!;

    // [Required(ErrorMessage = "Vai trò là bắt buộc")]
    // [RegularExpression("^(customer|seller|admin|manager)$", 
    //     ErrorMessage = "Vai trò phải là một trong các giá trị: customer, seller, admin, manager")]
    // public string Role { get; set; } = "customer";

    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 255 ký tự")]
    public string FullName { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [RegularExpression(@"^(\+84|84|0)?[3-9][0-9]{8}$", 
        ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
    public string? PhoneNumber { get; set; }

    // public bool IsVerified { get; set; } = false;

    // [StringLength(255, ErrorMessage = "Token xác minh không được vượt quá 255 ký tự")]
    // public string? VerificationToken { get; set; }
    
    // public DateTime? VerificationSentAt { get; set; }

    // [StringLength(500, ErrorMessage = "URL avatar không được vượt quá 500 ký tự")]
    // [Url(ErrorMessage = "URL avatar không đúng định dạng")]
    // public string? AvatarUrl { get; set; }

    // [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    // [RegularExpression("^(active|inactive|suspended|deleted)$", 
    //     ErrorMessage = "Trạng thái phải là một trong các giá trị: active, inactive, suspended, deleted")]
    // public string Status { get; set; } = "active";

    // public DateTime? LastLoginAt { get; set; }
    
    // public DateTime CreatedAt { get; set; }
    
    // public DateTime UpdatedAt { get; set; }
    
    // public DateTime? DeletedAt { get; set; }
}