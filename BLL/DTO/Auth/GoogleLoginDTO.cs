using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

/// <summary>
/// DTO for Google login request
/// </summary>
public class GoogleLoginDTO
{
    [Required(ErrorMessage = "Google ID Token is required")]
    public string IdToken { get; set; } = null!;
}

/// <summary>
/// DTO for Google user information from Google API
/// </summary>
public class GoogleUserInfoDTO
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Picture { get; set; } = null!;
    public bool EmailVerified { get; set; }
}