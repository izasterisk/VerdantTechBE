using System.ComponentModel;

namespace BLL.Helpers;

public static class Utils
{
    /// <summary>
    /// Chuyển đổi string thành enum với validation và error handling
    /// </summary>
    /// <typeparam name="TEnum">Kiểu enum cần chuyển đổi</typeparam>
    /// <param name="value">Giá trị string cần chuyển đổi</param>
    /// <param name="fieldName">Tên trường để hiển thị trong thông báo lỗi (tùy chọn)</param>
    /// <returns>Giá trị enum đã được chuyển đổi</returns>
    /// <exception cref="ArgumentException">Khi giá trị string không hợp lệ</exception>
    public static TEnum ParseEnum<TEnum>(string value, string? fieldName = null) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Giá trị {fieldName ?? typeof(TEnum).Name} không được để trống.");

        if (!Enum.TryParse<TEnum>(value, true, out var result))
        {
            var validValues = string.Join(", ", Enum.GetNames<TEnum>());
            var displayName = fieldName ?? GetEnumDisplayName<TEnum>();
            throw new ArgumentException($"Giá trị '{value}' không hợp lệ cho {displayName}. Các giá trị hợp lệ: {validValues}");
        }

        return result;
    }

    /// <summary>
    /// Lấy tên hiển thị của enum (sử dụng DisplayName attribute nếu có)
    /// </summary>
    /// <typeparam name="TEnum">Kiểu enum</typeparam>
    /// <returns>Tên hiển thị của enum</returns>
    private static string GetEnumDisplayName<TEnum>() where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var displayAttribute = type.GetCustomAttributes(typeof(DisplayNameAttribute), false)
            .FirstOrDefault() as DisplayNameAttribute;
        return displayAttribute?.DisplayName ?? type.Name;
    }
}