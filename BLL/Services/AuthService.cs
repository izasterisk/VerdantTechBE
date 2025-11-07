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

    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        
        var user = await _authRepository.GetUserByEmailAsync(loginDto.Email, cancellationToken);
        if(user == null)
            throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        AuthValidationHelper.ValidateUserStatus(user);
        AuthValidationHelper.ValidateLoginCredentials(user, loginDto.Password);

        var (token, refreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user!, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateUserWithTransactionAsync(user, cancellationToken);
        
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

    public async Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginDTO googleLoginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(googleLoginDto);
        
        // Validate Google ID Token
        var googleUser = await GoogleAuthHelper.ValidateGoogleTokenAsync(googleLoginDto.IdToken);
        
        // Check if user exists by email
        var user = await _authRepository.GetUserByEmailAsync(googleUser.Email, cancellationToken);
        if (user == null)
        {
            // Create new user with Google information
            user = GoogleAuthHelper.CreateUserFromGoogleAuth(googleUser);
            await _userRepository.CreateUserWithTransactionAsync(user, cancellationToken);
        }
        else
        {
            AuthValidationHelper.ValidateUserStatus(user);
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateUserWithTransactionAsync(user, cancellationToken);
        }
        
        // Generate tokens and update user
        var (token, refreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user, cancellationToken);

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

    public async Task<RefreshTokenResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        
        var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        AuthValidationHelper.ValidateRefreshToken(user);

        var (newToken, newRefreshToken, refreshTokenExpiry) = await GenerateTokensAndUpdateUserAsync(user!, cancellationToken);

        return new RefreshTokenResponseDTO
        {
            Token = newToken,
            TokenExpiresAt = DateTime.UtcNow.AddHours(_jwtExpireHours),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiry
        };
    }
    
    public async Task LogoutAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetUserWithAddressesByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        if (!string.IsNullOrEmpty(user.RefreshToken))
            await _authRepository.LogoutUserAsync(user, cancellationToken);
    }

    public async Task SendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken) 
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        AuthValidationHelper.ValidateUserStatus(user);
        if (user.IsVerified)
            throw new InvalidOperationException(AuthConstants.USER_ALREADY_VERIFIED);

        var code = AuthUtils.GenerateNumericCode(AuthConstants.VERIFICATION_CODE_LENGTH);
        await _emailSender.SendVerificationEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task SendForgotPasswordEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken) 
            ?? throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        AuthValidationHelper.ValidateUserStatus(user);
        var code = AuthUtils.GenerateNumericCode(AuthConstants.VERIFICATION_CODE_LENGTH);
        await _emailSender.SendForgotPasswordEmailAsync(user.Email, user.FullName, code);

        user.VerificationToken = code;
        user.VerificationSentAt = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken);
        AuthValidationHelper.ValidateVerificationCode(user, code);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.IsVerified = true;
        await _authRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task UpdateForgotPasswordAsync(string email, string newPassword, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        
        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken);
        AuthValidationHelper.ValidateResetPasswordCode(user, code);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task ChangePassword(string email, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetUserByEmailAsync(email, cancellationToken);
        AuthValidationHelper.ValidateOldPassword(user, oldPassword);
        AuthValidationHelper.ValidateUserStatus(user);
        user!.PasswordHash = AuthUtils.HashPassword(newPassword);
        await _authRepository.UpdateUserAsync(user, cancellationToken);
    }
    
    private async Task<(string token, string refreshToken, DateTime expiry)> GenerateTokensAndUpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = TokenHelper.GenerateJwtToken(user, _configuration);
        var refreshToken = TokenHelper.GenerateRefreshToken();
        var refreshTokenExpiry = TokenHelper.GetRefreshTokenExpiryTime();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiry;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateUserWithTransactionAsync(user, cancellationToken);

        return (token, refreshToken, refreshTokenExpiry);
    }
}