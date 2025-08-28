using BLL.DTO.Auth;

namespace BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(ulong userId);
    Task SendVerificationEmailAsync(string email);
}