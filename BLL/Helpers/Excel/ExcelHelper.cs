using OfficeOpenXml;
using System.Collections.Generic;
using System.Globalization;

namespace BLL.Helpers.Excel;

public static class ExcelHelper
{
    /// <summary>
    /// Đọc file Excel và trả về danh sách các dòng dữ liệu dưới dạng Dictionary
    /// </summary>
    /// <param name="stream">Stream của file Excel</param>
    /// <param name="sheetName">Tên sheet cần đọc (mặc định là sheet đầu tiên)</param>
    /// <param name="hasHeader">Có header row không (mặc định là true)</param>
    /// <returns>Danh sách các dòng dữ liệu, mỗi dòng là Dictionary với key là tên cột</returns>
    public static List<Dictionary<string, string>> ReadExcelFile(Stream stream, string? sheetName = null, bool hasHeader = true)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage(stream);
        
        var worksheet = sheetName != null
            ? package.Workbook.Worksheets[sheetName]
            : package.Workbook.Worksheets[0];

        if (worksheet == null)
            throw new InvalidOperationException("Không tìm thấy worksheet trong file Excel.");

        var result = new List<Dictionary<string, string>>();
        var startRow = hasHeader ? 2 : 1; // Bắt đầu từ dòng 2 nếu có header
        var endRow = worksheet.Dimension?.End.Row ?? 0;

        if (endRow == 0)
            return result;

        // Đọc header row nếu có
        Dictionary<int, string>? columnHeaders = null;
        if (hasHeader)
        {
            columnHeaders = new Dictionary<int, string>();
            var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
            foreach (var cell in headerRow)
            {
                if (cell.Value != null)
                {
                    var headerName = cell.Value.ToString()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(headerName))
                    {
                        columnHeaders[cell.Start.Column] = headerName;
                    }
                }
            }
        }

        // Đọc dữ liệu
        for (int row = startRow; row <= endRow; row++)
        {
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool hasData = false;

            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[row, col];
                var cellValue = cell.Value?.ToString()?.Trim() ?? string.Empty;

                if (!string.IsNullOrEmpty(cellValue))
                    hasData = true;

                string columnName;
                if (hasHeader && columnHeaders != null && columnHeaders.ContainsKey(col))
                {
                    columnName = columnHeaders[col];
                }
                else
                {
                    columnName = $"Column{col}";
                }

                rowData[columnName] = cellValue;
            }

            // Chỉ thêm dòng nếu có ít nhất một cell có dữ liệu
            if (hasData)
            {
                result.Add(rowData);
            }
        }

        return result;
    }

    /// <summary>
    /// Validate định dạng file Excel
    /// </summary>
    public static bool ValidateExcelFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension == ".xlsx" || extension == ".xls";
    }

    /// <summary>
    /// Parse giá trị từ Excel cell sang kiểu dữ liệu cụ thể
    /// </summary>
    public static T? ParseValue<T>(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default(T);

        var type = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        try
        {
            if (underlyingType == typeof(ulong))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    return (T)(object)Convert.ToUInt64(dec);
            }
            else if (underlyingType == typeof(int))
            {
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                    return (T)(object)intVal;
            }
            else if (underlyingType == typeof(decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decVal))
                    return (T)(object)decVal;
            }
            else if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateVal))
                    return (T)(object)dateVal;
            }
            else if (underlyingType == typeof(DateOnly))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateVal))
                    return (T)(object)DateOnly.FromDateTime(dateVal);
            }
            else if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(value, out var boolVal))
                    return (T)(object)boolVal;
            }
            else if (underlyingType == typeof(string))
            {
                return (T)(object)value;
            }

            return default(T);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Parse danh sách từ chuỗi phân cách bằng dấu phẩy
    /// </summary>
    public static List<string> ParseCommaSeparatedList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    /// <summary>
    /// Parse JSON string thành Dictionary
    /// </summary>
    public static Dictionary<string, object>? ParseJsonToDictionary(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
        }
        catch
        {
            return null;
        }
    }
}

