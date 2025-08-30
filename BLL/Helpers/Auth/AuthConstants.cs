namespace BLL.Helpers.Auth;

public static class AuthConstants
{
    // Token Configuration
    public const int DEFAULT_JWT_EXPIRE_HOURS = 24;
    public const int DEFAULT_REFRESH_TOKEN_EXPIRE_DAYS = 30;
    
    // Verification Configuration
    public const int VERIFICATION_CODE_LENGTH = 8;
    public const int VERIFICATION_CODE_EXPIRE_MINUTES = 10;
    
    // Environment Variable Keys
    public const string JWT_SECRET_KEY = "JWT_SECRET";
    public const string JWT_ISSUER_KEY = "JWT_ISSUER";
    public const string JWT_AUDIENCE_KEY = "JWT_AUDIENCE";
    public const string JWT_EXPIRE_HOURS_KEY = "JWT_EXPIRE_HOURS";
    public const string REFRESH_TOKEN_EXPIRE_DAYS_KEY = "REFRESH_TOKEN_EXPIRE_DAYS";
    public const string GOOGLE_CLIENT_ID_KEY = "GOOGLE_CLIENT_ID";
    
    // Error Messages
    public const string INVALID_EMAIL_OR_PASSWORD = "Invalid email or password";
    public const string EMAIL_NOT_VERIFIED = "Email not verified. Please enter your 8-digit verification code.";
    public const string USER_NOT_FOUND = "User not found";
    public const string USER_ALREADY_VERIFIED = "User already verified";
    public const string INVALID_VERIFICATION_CODE = "Invalid verification code";
    public const string VERIFICATION_CODE_EXPIRED = "Verification code expired";
    public const string INVALID_RESET_PASSWORD_CODE = "Invalid reset password code";
    public const string RESET_PASSWORD_CODE_EXPIRED = "Reset password code expired";
    public const string INVALID_OLD_PASSWORD = "Invalid old password";
    public const string INVALID_OR_EXPIRED_REFRESH_TOKEN = "Invalid or expired refresh token";
    public const string GOOGLE_CLIENT_ID_NOT_CONFIGURED = "Google Client ID not configured";
    public const string INVALID_GOOGLE_TOKEN = "Invalid Google token";
}
