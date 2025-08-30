using BLL.DTO.Auth;
using BLL.Helpers.Auth;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.IRepository;
using Microsoft.Extensions.Configuration;
using DAL.Data.Models;

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
        _jwtExpireHours = TokenHelper.GetJwtExpireHours();
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        
        var user = await _authRepository.GetUserByEmailAsync(loginDto.Email);
        AuthValidationHelper.ValidateUserStatus(user);
        AuthValidationHelper.ValidateLoginCredentials(user, loginDto.Password);

        var (token, refreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user!);

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

    public async Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDto)
    {
        ArgumentNullException.ThrowIfNull(googleLoginDto);
        
        // Validate Google ID Token
        var googleUser = await GoogleAuthHelper.ValidateGoogleTokenAsync(googleLoginDto.IdToken);
        
        // Check if user exists by email
        var user = await _authRepository.GetUserByEmailAsync(googleUser.Email);
        if (user == null)
        {
            // Create new user with Google information
            user = GoogleAuthHelper.CreateUserFromGoogleAuth(googleUser);
            await _userRepository.CreateUserWithTransactionAsync(user);
        }
        else
        {
            AuthValidationHelper.ValidateUserStatus(user);
        }
        
        // Generate tokens and update user
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
        
        var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken);
        AuthValidationHelper.ValidateRefreshToken(user);

        var (newToken, newRefreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user!);

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
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        if (!string.IsNullOrEmpty(user.RefreshToken))
            await _authRepository.LogoutUserAsync(user);
    }

    public async Task SendVerificationEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        AuthValidationHelper.ValidateUserStatus(user);
        if (user.IsVerified)
            throw new InvalidOperationException(AuthConstants.USER_ALREADY_VERIFIED);

        var code = AuthUtils.GenerateNumericCode(AuthConstants.VERIFICATION_CODE_LENGTH);
        await _emailSender.SendVerificationEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task SendForgotPasswordEmailAsync(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email) 
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        AuthValidationHelper.ValidateUserStatus(user);
        var code = AuthUtils.GenerateNumericCode(AuthConstants.VERIFICATION_CODE_LENGTH);
        await _emailSender.SendForgotPasswordEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task VerifyEmailAsync(string email, string code)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email);
        AuthValidationHelper.ValidateVerificationCode(user, code);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.IsVerified = true;
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task UpdateForgotPasswordAsync(string email, string newPassword, string code)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email);
        AuthValidationHelper.ValidateResetPasswordCode(user, code);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user);
    }

    public async Task ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = await _authRepository.GetUserByEmailAsync(email);
        AuthValidationHelper.ValidateOldPassword(user, oldPassword);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user);
    }
    
    private async Task<(string token, string refreshToken, DateTime expiry)> GenerateTokensAndUpdateUserAsync(User user)
    {
        var token = TokenHelper.GenerateJwtToken(user, _configuration);
        var refreshToken = TokenHelper.GenerateRefreshToken();
        var refreshTokenExpiry = TokenHelper.GetRefreshTokenExpiryTime();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserWithTransactionAsync(user);

        return (token, refreshToken, refreshTokenExpiry);
    }
}