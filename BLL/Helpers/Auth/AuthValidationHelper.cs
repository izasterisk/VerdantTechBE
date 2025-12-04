using DAL.Data;
using DAL.Data.Models;

namespace BLL.Helpers.Auth;

public static class AuthValidationHelper
{
    /// <summary>
    /// Validate user login credentials
    /// </summary>
    /// <param name="user">User from database</param>
    /// <param name="password">Plain text password</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when email is not verified</exception>
    public static void ValidateLoginCredentials(User? user, string password)
    {
        if (user == null)
            throw new UnauthorizedAccessException(AuthConstants.INVALID_EMAIL_OR_PASSWORD);
        
        if (!AuthUtils.VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException(AuthConstants.INVALID_EMAIL_OR_PASSWORD);

        if (!user.IsVerified)
            throw new InvalidOperationException(AuthConstants.EMAIL_NOT_VERIFIED);
    }
    
    /// <summary>
    /// Validate user existence and status
    /// </summary>
    /// <param name="user">User from database</param>
    /// <exception cref="Exception">Thrown when user is not found</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is deleted or suspended</exception>
    public static void ValidateUserStatus(User? user)
    {
        if (user == null)
            throw new Exception(AuthConstants.USER_NOT_FOUND);
        
        if (user.Status == UserStatus.Deleted)
            throw new UnauthorizedAccessException(AuthConstants.USER_DELETED);
    
        if (user.Status == UserStatus.Suspended)
            throw new UnauthorizedAccessException(AuthConstants.USER_SUSPENDED);
        
        if (user.Status == UserStatus.Inactive)
            throw new UnauthorizedAccessException("Người dùng bị vô hiệu hóa.");
    }

    /// <summary>
    /// Validate email verification code
    /// </summary>
    /// <param name="user">User from database</param>
    /// <param name="code">Verification code</param>
    /// <exception cref="InvalidOperationException">Thrown when user not found or already verified</exception>
    /// <exception cref="ArgumentException">Thrown when code is invalid</exception>
    public static void ValidateVerificationCode(User? user, string code)
    {
        if (user == null)
            throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        if (user.IsVerified)
            throw new InvalidOperationException(AuthConstants.USER_ALREADY_VERIFIED);
        
        if (user.VerificationToken != code)
            throw new ArgumentException(AuthConstants.INVALID_VERIFICATION_CODE);
        
        if (IsVerificationCodeExpired(user.VerificationSentAt))
            throw new InvalidOperationException(AuthConstants.VERIFICATION_CODE_EXPIRED);
    }

    /// <summary>
    /// Validate forgot password reset code
    /// </summary>
    /// <param name="user">User from database</param>
    /// <param name="code">Reset password code</param>
    /// <exception cref="InvalidOperationException">Thrown when user not found</exception>
    /// <exception cref="ArgumentException">Thrown when code is invalid</exception>
    public static void ValidateResetPasswordCode(User? user, string code)
    {
        if (user == null)
            throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        if (user.VerificationToken != code)
            throw new ArgumentException(AuthConstants.INVALID_RESET_PASSWORD_CODE);
        
        if (IsVerificationCodeExpired(user.VerificationSentAt))
            throw new InvalidOperationException(AuthConstants.RESET_PASSWORD_CODE_EXPIRED);
    }

    /// <summary>
    /// Validate old password for password change
    /// </summary>
    /// <param name="user">User from database</param>
    /// <param name="oldPassword">Old password</param>
    /// <exception cref="InvalidOperationException">Thrown when user not found</exception>
    /// <exception cref="ArgumentException">Thrown when old password is invalid</exception>
    public static void ValidateOldPassword(User? user, string oldPassword)
    {
        if (user == null)
            throw new InvalidOperationException(AuthConstants.USER_NOT_FOUND);
        
        if (!AuthUtils.VerifyPassword(oldPassword, user.PasswordHash))
            throw new ArgumentException(AuthConstants.INVALID_OLD_PASSWORD);
    }

    /// <summary>
    /// Validate refresh token
    /// </summary>
    /// <param name="user">User from database</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when refresh token is invalid or expired</exception>
    public static void ValidateRefreshToken(User? user)
    {
        if (user == null)
            throw new UnauthorizedAccessException(AuthConstants.INVALID_OR_EXPIRED_REFRESH_TOKEN);
    }

    /// <summary>
    /// Check if verification code is expired
    /// </summary>
    /// <param name="verificationSentAt">When verification was sent</param>
    /// <returns>True if expired, false otherwise</returns>
    private static bool IsVerificationCodeExpired(DateTime? verificationSentAt)
    {
        return verificationSentAt is null || 
               verificationSentAt < DateTime.UtcNow.AddMinutes(-AuthConstants.VERIFICATION_CODE_EXPIRE_MINUTES);
    }
}
