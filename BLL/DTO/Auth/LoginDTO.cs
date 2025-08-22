using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class LoginDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = null!;
}

public class LoginResponseDTO
{
    public string Token { get; set; } = null!;
    public DateTime TokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserInfoDTO User { get; set; } = null!;
}

public class UserInfoDTO
{
    public ulong Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }
}