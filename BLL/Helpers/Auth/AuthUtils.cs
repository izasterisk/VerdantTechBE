using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Helpers.Auth;

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
    /// Validate vendor subscription by checking the latest completed subscription transaction
    /// </summary>
    /// <param name="authRepository">Auth repository instance</param>
    /// <param name="vendorId">Vendor user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The latest subscription transaction</returns>
    /// <exception cref="ForbiddenException">Thrown when vendor has no active subscription</exception>
    public static async Task ValidateVendorSubscriptionAsync(IAuthRepository authRepository,
        VendorProfile vendorProfile, CancellationToken cancellationToken = default)
    {
        var subscription = await authRepository.ValidateVendorSubscriptionAsync(vendorProfile.UserId, cancellationToken);
        bool check = false;
        if (subscription != null)
        {
            if(subscription.Note == "12MONTHS" && subscription.CreatedAt.AddMonths(12) > DateTime.UtcNow)
                check = true;
            if(subscription.Note == "6MONTHS" && subscription.CreatedAt.AddMonths(6) > DateTime.UtcNow)
                check = true;
        }
        if (vendorProfile.SubscriptionActive != check)
        {
            vendorProfile.SubscriptionActive = check;
            await authRepository.UpdateVendorProfileAsync(vendorProfile, cancellationToken);
        }
    }
}