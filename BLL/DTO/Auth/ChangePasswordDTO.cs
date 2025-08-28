using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Old password is required")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = null!;
}