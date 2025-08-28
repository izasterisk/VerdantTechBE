using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class VerifyEmailDTO
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(8)]
    public string Code { get; set; } = null!;
}


