using BLL.DTO.Auth;

namespace BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto);
    Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(ulong userId);
    Task SendVerificationEmailAsync(string email);
    Task VerifyEmailAsync(string email, string code);
    Task SendForgotPasswordEmailAsync(string email);
    Task UpdateForgotPasswordAsync(string email, string newPassword, string code);
    Task ChangePassword(string email, string oldPassword, string newPassword);
}