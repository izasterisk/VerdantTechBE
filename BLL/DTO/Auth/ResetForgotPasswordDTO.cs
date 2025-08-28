using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class ResetForgotPasswordDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Change password code is required")]
    [StringLength(8)]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = null!;
}
