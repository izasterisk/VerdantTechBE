using System.ComponentModel.DataAnnotations;

namespace BLL.DTO.Auth;

public class RefreshTokenDTO
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}

public class RefreshTokenResponseDTO
{
    public string Token { get; set; } = null!;
    public DateTime TokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiresAt { get; set; }
    
}
