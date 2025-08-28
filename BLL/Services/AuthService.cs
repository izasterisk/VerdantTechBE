using BLL.DTO.Auth;
using BLL.Interfaces;
using BLL.Utils;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;
using Microsoft.Extensions.Configuration;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    
    // Cache JWT expire hours to avoid repeated environment variable lookups
    private readonly int _jwtExpireHours;
    
    public AuthService(IAuthRepository authRepository, IConfiguration configuration, 
        IUserRepository userRepository, IEmailSender emailSender)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _userRepository = userRepository;
        _emailSender = emailSender;
        _jwtExpireHours = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS"), out var hours) 
            ? hours : 24;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        
        var user = await _authRepository.GetUserByEmailAsync(loginDto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password");
        
        if (!AuthUtils.VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsVerified)
            throw new InvalidOperationException("Email not verified. Please enter your 8-digit verification code.");

        var (token, refreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user);

        return new LoginResponseDTO
        {
            Token = token,
            TokenExpiresAt = DateTime.UtcNow.AddHours(_jwtExpireHours),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiry,
            User = new UserInfoDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                IsVerified = user.IsVerified
            }
        };
    }

    public async Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        
        var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var (newToken, newRefreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user);

        return new RefreshTokenResponseDTO
        {
            Token = newToken,
            TokenExpiresAt = DateTime.UtcNow.AddHours(_jwtExpireHours),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };
    }
    
    public async Task LogoutAsync(ulong userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");
        
        if (!string.IsNullOrEmpty(user.RefreshToken))
            await _authRepository.LogoutUserAsync(user);
    }

    public async Task SendVerificationEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
            ?? throw new InvalidOperationException("User not found");
        
        if (user.IsVerified)
            throw new InvalidOperationException("User already verified");

        var code = AuthUtils.GenerateNumericCode();
        await _emailSender.SendVerificationEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task SendForgotPasswordEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
            ?? throw new InvalidOperationException("User not found");

        var code = AuthUtils.GenerateNumericCode();
        await _emailSender.SendForgotPasswordEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task VerifyEmailAsync(string email, string code)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
            ?? throw new InvalidOperationException("User not found");
        
        if (user.IsVerified)
            throw new InvalidOperationException("User already verified");
        if (user.VerificationToken != code)
            throw new ArgumentException("Invalid verification code");
        if (user.VerificationSentAt is null || user.VerificationSentAt < DateTime.UtcNow.AddMinutes(-10))
            throw new InvalidOperationException("Verification code expired");
        
        user.IsVerified = true;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task UpdateForgotPasswordAsync(string email, string newPassword, string code)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
                   ?? throw new InvalidOperationException("User not found");
        if (user.VerificationToken != code)
            throw new ArgumentException("Invalid reset password code");
        if (user.VerificationSentAt is null || user.VerificationSentAt < DateTime.UtcNow.AddMinutes(-10))
            throw new InvalidOperationException("Reset password code code expired");
        user.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = await _authRepository.GetUserByEmailAsync(email) 
                   ?? throw new InvalidOperationException("User not found");
        if (!AuthUtils.VerifyPassword(oldPassword, user.PasswordHash))
            throw new ArgumentException("Invalid old password");
        user.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user);
    }
    
    private async Task<(string token, string refreshToken, DateTime expiry)> GenerateTokensAndUpdateUserAsync(dynamic user)
    {
        var token = AuthUtils.GenerateJwtToken(user, _configuration);
        var refreshToken = AuthUtils.GenerateRefreshToken();
        var refreshTokenExpiry = AuthUtils.GetRefreshTokenExpiryTime();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserWithTransactionAsync(user);

        return (token, refreshToken, refreshTokenExpiry);
    }
}