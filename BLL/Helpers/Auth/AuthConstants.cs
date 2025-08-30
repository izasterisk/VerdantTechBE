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
    public const string INVALID_EMAIL_OR_PASSWORD = "Email hoặc mật khẩu không hợp lệ";
    public const string EMAIL_NOT_VERIFIED = "Email chưa được xác minh. Vui lòng nhập mã xác minh 8 chữ số.";
    public const string USER_NOT_FOUND = "Không tìm thấy người dùng";
    public const string USER_ALREADY_VERIFIED = "Người dùng đã được xác minh";
    public const string INVALID_VERIFICATION_CODE = "Mã xác minh không hợp lệ";
    public const string VERIFICATION_CODE_EXPIRED = "Mã xác minh đã hết hạn";
    public const string INVALID_RESET_PASSWORD_CODE = "Mã đặt lại mật khẩu không hợp lệ";
    public const string RESET_PASSWORD_CODE_EXPIRED = "Mã đặt lại mật khẩu đã hết hạn";
    public const string INVALID_OLD_PASSWORD = "Mật khẩu cũ không hợp lệ";
    public const string INVALID_OR_EXPIRED_REFRESH_TOKEN = "Mã thông báo làm mới không hợp lệ hoặc đã hết hạn";
    public const string GOOGLE_CLIENT_ID_NOT_CONFIGURED = "Chưa cấu hình Google Client ID";
    public const string INVALID_GOOGLE_TOKEN = "Mã thông báo Google không hợp lệ";
    public const string USER_DELETED = "Người dùng đã bị xóa";
    public const string USER_INACTIVE = "Người dùng không hoạt động";
    public const string USER_SUSPENDED = "Người dùng bị đình chỉ";
}
