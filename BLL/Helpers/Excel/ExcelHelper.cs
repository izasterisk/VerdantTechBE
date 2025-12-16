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
        if (stream == null)
            throw new ArgumentNullException(nameof(stream), "Stream không được null.");

        if (!stream.CanRead)
            throw new InvalidOperationException("Stream không thể đọc được.");

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        // Copy stream vào memory để tránh dispose và có thể đọc lại
        byte[] fileBytes;
        try
        {
            if (stream is MemoryStream ms && ms.CanSeek && ms.CanRead)
            {
                // Nếu đã là MemoryStream và có thể seek, chỉ cần reset position và copy
                ms.Position = 0;
                fileBytes = ms.ToArray();
            }
            else
            {
                // Copy stream vào memory
                using var tempStream = new MemoryStream();
                if (stream.CanSeek)
                    stream.Position = 0;
                stream.CopyTo(tempStream);
                fileBytes = tempStream.ToArray();
            }

            if (fileBytes == null || fileBytes.Length == 0)
                throw new InvalidOperationException("File Excel rỗng hoặc không có dữ liệu.");
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Lỗi khi đọc stream: {ex.Message}", ex);
        }
        
        // Sử dụng fileBytes để tạo MemoryStream mới
        using var memoryStream = new MemoryStream(fileBytes);
        ExcelPackage package;
        try
        {
            package = new ExcelPackage(memoryStream);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi khi mở file Excel. File có thể bị hỏng hoặc không đúng định dạng: {ex.Message}", ex);
        }

        using (package)
        {
            ExcelWorksheet? worksheet;
            try
            {
                worksheet = sheetName != null
                    ? package.Workbook.Worksheets[sheetName]
                    : package.Workbook.Worksheets[0];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi truy cập worksheet: {ex.Message}", ex);
            }

            if (worksheet == null)
                throw new InvalidOperationException("Không tìm thấy worksheet trong file Excel.");

            var dimension = worksheet.Dimension;
            if (dimension == null)
                return new List<Dictionary<string, string>>();

            var result = new List<Dictionary<string, string>>();
            var startRow = hasHeader ? 2 : 1; // Bắt đầu từ dòng 2 nếu có header
            var endRow = dimension.End.Row;

            if (endRow == 0)
                return result;

            // Đọc header row nếu có
            Dictionary<int, string>? columnHeaders = null;
            if (hasHeader)
            {
                try
                {
                    columnHeaders = new Dictionary<int, string>();
                    var headerRow = worksheet.Cells[1, 1, 1, dimension.End.Column];
                    foreach (var cell in headerRow)
                    {
                        if (cell.Value != null)
                        {
                            var headerName = cell.Value.ToString()?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(headerName))
                            {
                                // Normalize tên cột: bỏ khoảng trắng và underscore để tránh lỗi khi parse
                                // Ví dụ: "Length Cm" → "LengthCm", "Length_Cm" → "LengthCm"
                                var normalizedName = headerName.Replace(" ", "").Replace("_", "");
                                columnHeaders[cell.Start.Column] = normalizedName;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Lỗi khi đọc header row: {ex.Message}", ex);
                }
            }

            // Đọc dữ liệu
            try
            {
                for (int row = startRow; row <= endRow; row++)
                {
                    var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    bool hasData = false;

                    for (int col = 1; col <= dimension.End.Column; col++)
                    {
                        try
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
                        catch (Exception ex)
                        {
                            // Log warning nhưng tiếp tục đọc các cell khác
                            rowData[$"Column{col}"] = string.Empty;
                        }
                    }

                    // Chỉ thêm dòng nếu có ít nhất một cell có dữ liệu
                    if (hasData)
                    {
                        result.Add(rowData);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi đọc dữ liệu từ Excel: {ex.Message}", ex);
            }

            return result;
        }
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
    /// Parse giá trị từ Excel cell sang kiểu dữ liệu cụ thể (nullable)
    /// </summary>
    public static T? ParseValue<T>(string? value) where T : struct
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var underlyingType = typeof(T);

        try
        {
            if (underlyingType == typeof(ulong))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    return (T?)(object)Convert.ToUInt64(dec);
            }
            else if (underlyingType == typeof(int))
            {
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                    return (T?)(object)intVal;
            }
            else if (underlyingType == typeof(decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decVal))
                    return (T?)(object)decVal;
            }
            else if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateVal))
                    return (T?)(object)dateVal;
            }
            else if (underlyingType == typeof(DateOnly))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateVal))
                    return (T?)(object)DateOnly.FromDateTime(dateVal);
            }
            else if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(value, out var boolVal))
                    return (T?)(object)boolVal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse giá trị string từ Excel cell
    /// </summary>
    public static string? ParseValueString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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

