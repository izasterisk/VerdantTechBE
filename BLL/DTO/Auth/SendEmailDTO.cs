using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class SendEmailDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = null!;
}


