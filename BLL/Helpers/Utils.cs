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
}