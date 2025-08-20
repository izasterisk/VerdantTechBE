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

    public AuthService(IAuthRepository authRepository, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
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
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = AuthUtils.GetRefreshTokenExpiryTime();
            user.LastLoginAt = DateTime.UtcNow;
            
            await _authRepository.UpdateUserAsync(user);

            var jwtExpireHours = Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24";
            var loginResponse = new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtExpireHours)),
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
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = AuthUtils.GetRefreshTokenExpiryTime();
            
            await _authRepository.UpdateUserAsync(user);

            var jwtExpireHours = Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24";
            var refreshResponse = new RefreshTokenResponseDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtExpireHours))
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
}