using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using DAL.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Utils;

public static class AuthUtils
{
    /// <summary>
    /// Generate numeric verification code with cryptographic RNG
    /// </summary>
    /// <param name="length">Desired length, default 8</param>
    /// <returns>Numeric string</returns>
    public static string GenerateNumericCode(int length = 8)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[length];

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        Span<byte> buffer = stackalloc byte[length];
        rng.GetBytes(buffer);

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[buffer[i] % chars.Length];
        }

        return new string(result);
    }
    /// <summary>
    /// Hash password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hashedPassword">Hashed password from database</param>
    /// <returns>True if password matches</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    /// <param name="user">User object</param>
    /// <param name="configuration">Configuration for JWT settings (not used, kept for compatibility)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateJwtToken(User user, IConfiguration configuration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = Encoding.ASCII.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT_SECRET not configured"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("verified", user.IsVerified.ToString().ToLower())
        };

        var jwtExpireHours = Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS") ?? "24";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(jwtExpireHours)),
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
        // Generate a secure refresh token
        var randomBytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Get refresh token expiry time
    /// </summary>
    /// <returns>DateTime for refresh token expiry</returns>
    public static DateTime GetRefreshTokenExpiryTime()
    {
        var refreshTokenExpireDays = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRE_DAYS") ?? "30";
        return DateTime.UtcNow.AddDays(Convert.ToDouble(refreshTokenExpireDays));
    }

    /// <summary>
    /// Validate and decode JWT token without throwing exceptions
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
    public static ClaimsPrincipal? ValidateTokenSilently(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            
            if (string.IsNullOrEmpty(jwtSecret))
                return null;

            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                ValidIssuer = jwtIssuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}