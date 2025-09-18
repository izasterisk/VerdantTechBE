using BLL.DTO.Auth;

namespace BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto, CancellationToken cancellationToken = default);
    Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDto, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(ulong userId, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);
    Task SendForgotPasswordEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateForgotPasswordAsync(string email, string newPassword, string code, CancellationToken cancellationToken = default);
    Task ChangePassword(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}