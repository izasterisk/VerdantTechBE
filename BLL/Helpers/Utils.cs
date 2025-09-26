using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL.Helpers;

public static class Utils
{
    private static readonly Regex RemoveMarks = new(@"\p{Mn}+", RegexOptions.Compiled);  // bỏ dấu kết hợp
    private static readonly Regex NonAlnumToDash = new(@"[^a-z0-9]+", RegexOptions.Compiled); // gom thành '-'

    /// <summary>
    /// Tạo slug từ chuỗi đầu vào (chuyển thành URL-friendly string)
    /// </summary>
    /// <param name="input">Chuỗi đầu vào cần chuyển thành slug</param>
    /// <returns>Chuỗi slug đã được xử lý</returns>
    public static string GenerateSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = input.Replace('đ', 'd').Replace('Đ', 'D');
        var s = input.Normalize(NormalizationForm.FormD);
        s = RemoveMarks.Replace(s, "");          // bỏ dấu
        s = s.ToLowerInvariant();
        s = NonAlnumToDash.Replace(s, "-");      // mọi thứ không phải a-z0-9 -> '-'
        s = s.Trim('-');

        // Giới hạn độ dài tối đa 255 ký tự, cắt ở dấu '-' gần nhất
        if (s.Length > 255)
        {
            var cut = s.LastIndexOf('-', 255);
            if (cut > 0)
                s = s.Substring(0, cut);
            else
                s = s.Substring(0, 255);
            s = s.Trim('-');
        }

        return s;
    }
    
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