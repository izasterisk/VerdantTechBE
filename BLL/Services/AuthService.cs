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
            await _authRepository.UpdateUserAsync(user);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

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

    private string GenerateJwtToken(DAL.Data.Models.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET not configured"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("verified", user.IsVerified.ToString().ToLower())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JWT_EXPIRE_HOURS"] ?? "24")),
            Issuer = _configuration["JWT_ISSUER"],
            Audience = _configuration["JWT_AUDIENCE"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        // Generate a simple refresh token - in production, this should be more secure
        return Guid.NewGuid().ToString();
    }
}