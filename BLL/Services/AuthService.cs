using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
    private readonly ICustomerRepository _customerRepository;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration, ICustomerRepository customerRepository)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _customerRepository = customerRepository;
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
                    Data = null,
                    Errors = new List<string> { "Invalid email or password" }
                };
            }

            // Check if user is active
            if (user.Status != DAL.Data.UserStatus.Active)
            {
                return new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    Data = null,
                    Errors = new List<string> { "Account is not active" }
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _customerRepository.UpdateCustomerWithTransactionAsync(user);

            // Generate JWT token
            var token = AuthUtils.GenerateJwtToken(user, _configuration);
            var refreshToken = AuthUtils.GenerateRefreshToken();

            var loginResponse = new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JWT_EXPIRE_HOURS"] ?? "24")),
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
                Data = null,
                Errors = new List<string> { "An error occurred during login", ex.Message }
            };
        }
    }

    public async Task<APIResponse> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT_ISSUER"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT_AUDIENCE"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return new APIResponse
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = "Token is valid",
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            return new APIResponse
            {
                Status = false,
                StatusCode = HttpStatusCode.Unauthorized,
                Data = null,
                Errors = new List<string> { "Invalid token", ex.Message }
            };
        }
    }

    public async Task<APIResponse> RefreshTokenAsync(string refreshToken)
    {
        // Implementation for refresh token logic
        // This is a placeholder - you would need to implement refresh token storage and validation
        await Task.CompletedTask;
        
        return new APIResponse
        {
            Status = false,
            StatusCode = HttpStatusCode.NotImplemented,
            Data = null,
            Errors = new List<string> { "Refresh token functionality not implemented yet" }
        };
    }
}