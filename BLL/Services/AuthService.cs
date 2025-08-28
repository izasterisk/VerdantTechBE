using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BLL.DTO.Auth;
using BLL.Interfaces;
using BLL.Utils;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    
    public AuthService(IAuthRepository authRepository, IConfiguration configuration, IUserRepository userRepository, IEmailSender emailSender)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto, $"{nameof(loginDto)} is null");
        
        // Validate user credentials
        var user = await _authRepository.GetUserByEmailAsync(loginDto.Email);
        
        if (user == null || !AuthUtils.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password");
        }

        // Block login until email is verified
        if (!user.IsVerified)
        {
            throw new Exception("Email not verified. Please enter your 8-digit verification code.");
        }

        // Generate tokens
        var token = AuthUtils.GenerateJwtToken(user, _configuration);
        var refreshToken = AuthUtils.GenerateRefreshToken();

        // Update user with refresh token and last login
        DateTime refTokenExpiry = AuthUtils.GetRefreshTokenExpiryTime();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refTokenExpiry;
        user.LastLoginAt = DateTime.UtcNow;
        
        await _authRepository.UpdateUserAsync(user);

        var jwtExpireHours = Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24";
        var loginResponse = new LoginResponseDTO
        {
            Token = token,
            TokenExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtExpireHours)),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refTokenExpiry,
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

        return loginResponse;
    }

    public async Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken, $"{nameof(refreshToken)} is null");
        
        // Find user by refresh token
        var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken);
        
        if (user == null)
        {
            throw new Exception("Invalid or expired refresh token");
        }

        // Generate new tokens
        var newToken = AuthUtils.GenerateJwtToken(user, _configuration);
        var newRefreshToken = AuthUtils.GenerateRefreshToken();

        // Update user with new refresh token
        DateTime refTokenExpiry = AuthUtils.GetRefreshTokenExpiryTime();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = refTokenExpiry;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateUserWithTransactionAsync(user);

        var jwtExpireHours = Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24";
        var refreshResponse = new RefreshTokenResponseDTO
        {
            Token = newToken,
            TokenExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtExpireHours)),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refTokenExpiry
        };

        return refreshResponse;
    }
    
    public async Task LogoutAsync(ulong userId)
    {
        // Find user by ID
        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            throw new Exception("User not found");
        }
        // Check if user is already logged out (no refresh token)
        if (string.IsNullOrEmpty(user.RefreshToken))
        {
            return; // User already logged out, no need to throw exception
        }
        // Logout user by clearing refresh token
        await _authRepository.LogoutUserAsync(user);
    }

    public async Task SendVerificationEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email, $"{nameof(email)} is null");
        
        var user = await _authRepository.GetUserByEmailAsync(email) ?? throw new Exception("User not found");
        if (user.IsVerified)
        {
            throw new Exception("User already verified");
        }

        // Generate 8-digit numeric code
        var code = AuthUtils.GenerateNumericCode(8);

        await _emailSender.SendVerificationEmailAsync(user.Email, user.FullName, code);

        // Only update database if email was sent successfully
        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
    }
}