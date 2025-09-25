using System.ComponentModel;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Tự động tạo slug từ chuỗi đầu vào, bao gồm việc chuyển đổi chữ thường, loại bỏ dấu tiếng Việt, thay khoảng trắng bằng dấu gạch ngang và loại bỏ ký tự đặc biệt
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ConvertToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = input.ToLower();

        input = RemoveVietnameseAccents(input);

        input = Regex.Replace(input, @"[^a-z0-9\s-]", "");

        input = Regex.Replace(input, @"\s+", " ").Trim();
        input = input.Replace(" ", "-");

        return input;
    }

    /// <summary>
    /// Hỗ trợ cho hàm ConvertToSlug, loại bỏ dấu tiếng Việt
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string RemoveVietnameseAccents(string input)
    {
        string[] vietnameseChars = new string[] { "aàáảãạăằắẳẵặâầấẩẫậ", "eèéẻẽẹêềếểễệ", "iìíỉĩị", "oòóỏõọôồốổỗộơờớởỡợ", "uùúủũụưừứửữự", "yỳýỷỹỵ", "dđ" };
        string[] replacementChars = new string[] { "a", "e", "i", "o", "u", "y", "d" };

        for (int i = 0; i < vietnameseChars.Length; i++)
        {
            foreach (var c in vietnameseChars[i])
            {
                input = input.Replace(c.ToString(), replacementChars[i]);
            }
        }

        return input;
    }

}