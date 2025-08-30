using BLL.DTO.Auth;
using Google.Apis.Auth;
using DAL.Data.Models;
using DAL.Data;

namespace BLL.Helpers.Auth;

public static class GoogleAuthHelper
{
    /// <summary>
    /// Validate Google ID token and extract user information
    /// </summary>
    /// <param name="idToken">Google ID token</param>
    /// <returns>Google user information</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when token is invalid</exception>
    public static async Task<GoogleUserInfoDTO> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var googleClientId = Environment.GetEnvironmentVariable(AuthConstants.GOOGLE_CLIENT_ID_KEY);
            if (string.IsNullOrEmpty(googleClientId))
                throw new InvalidOperationException(AuthConstants.GOOGLE_CLIENT_ID_NOT_CONFIGURED);

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { googleClientId }
            });

            return new GoogleUserInfoDTO
            {
                Id = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                Picture = payload.Picture,
                EmailVerified = payload.EmailVerified
            };
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Token Google không hợp lệ: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new user from Google authentication data
    /// </summary>
    /// <param name="googleUser">Google user information</param>
    /// <returns>New user object</returns>
    public static User CreateUserFromGoogleAuth(GoogleUserInfoDTO googleUser)
    {
        return new User
        {
            Email = googleUser.Email,
            FullName = googleUser.Name,
            PasswordHash = AuthUtils.HashPassword(Guid.NewGuid().ToString()), // Random password for Google users
            IsVerified = true, // Auto-verify Google users
            AvatarUrl = googleUser.Picture,
            Role = UserRole.Customer,
        };
    }
}
