using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAL.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Helpers.Auth;

public static class TokenHelper
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    /// <param name="user">User object</param>
    /// <param name="configuration">Configuration for JWT settings</param>
    /// <returns>JWT token string</returns>
    public static string GenerateJwtToken(User user, IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = Environment.GetEnvironmentVariable(AuthConstants.JWT_SECRET_KEY);
        var key = Encoding.ASCII.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT_SECRET not configured"));

        var claims = BuildUserClaims(user);
        var jwtExpireHours = GetJwtExpireHours();
        var jwtIssuer = Environment.GetEnvironmentVariable(AuthConstants.JWT_ISSUER_KEY);
        var jwtAudience = Environment.GetEnvironmentVariable(AuthConstants.JWT_AUDIENCE_KEY);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(jwtExpireHours),
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generate refresh token
    /// </summary>
    /// <returns>Refresh token string</returns>
    public static string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Get refresh token expiry time
    /// </summary>
    /// <returns>DateTime for refresh token expiry</returns>
    public static DateTime GetRefreshTokenExpiryTime()
    {
        var refreshTokenExpireDays = Environment.GetEnvironmentVariable(AuthConstants.REFRESH_TOKEN_EXPIRE_DAYS_KEY) 
            ?? AuthConstants.DEFAULT_REFRESH_TOKEN_EXPIRE_DAYS.ToString();
        return DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpireDays));
    }

    /// <summary>
    /// Get JWT token expiry hours from environment or default
    /// </summary>
    /// <returns>JWT expiry hours</returns>
    public static int GetJwtExpireHours()
    {
        return int.TryParse(Environment.GetEnvironmentVariable(AuthConstants.JWT_EXPIRE_HOURS_KEY), out var hours) 
            ? hours : AuthConstants.DEFAULT_JWT_EXPIRE_HOURS;
    }

    /// <summary>
    /// Build user claims for JWT token
    /// </summary>
    /// <param name="user">User object</param>
    /// <returns>List of claims</returns>
    private static List<Claim> BuildUserClaims(User user)
    {
        return new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };
    }
}
