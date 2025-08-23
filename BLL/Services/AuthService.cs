using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BLL.DTO;
using BLL.DTO.Auth;
using BLL.Interfaces;
using BLL.Utils;
using DAL.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    
    public AuthService(IAuthRepository authRepository, IConfiguration configuration, IUserRepository userRepository)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public async Task<APIResponse> LoginAsync(LoginDTO loginDto)
    {
        try
        {
            // Validate user credentials
            var user = await _authRepository.GetUserByEmailAsync(loginDto.Email);
            
            if (user == null || !AuthUtils.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    Data = null!,
                    Errors = new List<string> { "Invalid email or password" }
                };
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

            return new APIResponse
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = loginResponse,
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            return new APIResponse
            {
                Status = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Data = null!,
                Errors = new List<string> { "An error occurred during login", ex.Message }
            };
        }
    }

    public async Task<APIResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find user by refresh token
            var user = await _authRepository.GetUserByRefreshTokenAsync(refreshToken);
            
            if (user == null)
            {
                return new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    Data = null!,
                    Errors = new List<string> { "Invalid or expired refresh token" }
                };
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

            return new APIResponse
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = refreshResponse,
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            return new APIResponse
            {
                Status = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Data = null!,
                Errors = new List<string> { "An error occurred during token refresh", ex.Message }
            };
        }
    }
    
    public async Task<APIResponse> LogoutAsync(ulong userId)
    {
        try
        {
            // Find user by ID
            var user = await _userRepository.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Data = null!,
                    Errors = new List<string> { "User not found" }
                };
            }

            // Check if user is already logged out (no refresh token)
            if (string.IsNullOrEmpty(user.RefreshToken))
            {
                return new APIResponse
                {
                    Status = true,
                    StatusCode = HttpStatusCode.OK,
                    Data = "User already logged out",
                    Errors = new List<string>()
                };
            }

            // Logout user by clearing refresh token
            await _authRepository.LogoutUserAsync(user);

            return new APIResponse
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = "Logged out successfully",
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            return new APIResponse
            {
                Status = false,
                StatusCode = HttpStatusCode.InternalServerError,
                Data = null!,
                Errors = new List<string> { "An error occurred during logout", ex.Message }
            };
        }
    }
}