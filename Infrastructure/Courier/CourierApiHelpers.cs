using System.Text.Json;

namespace Infrastructure.Courier;

public static class CourierApiHelpers
{
    /// <summary>
    /// Parse GHN API response và trích xuất data
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về sau khi map</typeparam>
    /// <param name="jsonContent">JSON response từ GHN API</param>
    /// <param name="mapper">Hàm mapper để convert JsonElement thành T</param>
    /// <param name="contextMessage">Message context cho error (vd: "dịch vụ vận chuyển", "thời gian giao hàng")</param>
    /// <returns>Kết quả sau khi map</returns>
    /// <exception cref="InvalidOperationException">Khi response không hợp lệ hoặc có lỗi</exception>
    public static T ParseGhnResponse<T>(string jsonContent, Func<JsonElement, T> mapper, string contextMessage)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            throw new InvalidOperationException($"Response rỗng khi lấy dữ liệu {contextMessage}.");
        }

        // Check if response is HTML (error page) instead of JSON
        if (jsonContent.TrimStart().StartsWith("<"))
        {
            throw new InvalidOperationException($"API endpoint không khả dụng hoặc URL không đúng khi lấy {contextMessage}.");
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;

            // Check response code
            if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
            {
                var message = root.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() 
                    : "Unknown error";
                throw new InvalidOperationException($"GHN API error khi lấy {contextMessage}: {message}");
            }

            // Extract data property
            if (!root.TryGetProperty("data", out var dataElement))
            {
                throw new InvalidOperationException($"Response không có trường 'data' khi lấy {contextMessage}.");
            }

            // Handle null data (error case)
            if (dataElement.ValueKind == JsonValueKind.Null)
            {
                throw new InvalidOperationException($"Dữ liệu {contextMessage} không tồn tại (data is null).");
            }

            // Apply mapper function
            return mapper(dataElement);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể parse JSON response từ GHN API khi lấy {contextMessage}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate và check JsonElement có phải là array hợp lệ không
    /// </summary>
    public static void ValidateArrayData(JsonElement dataElement, string contextMessage)
    {
        if (dataElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"Dữ liệu {contextMessage} không đúng định dạng (không phải array).");
        }
    }
    
    /// <summary>
    /// Validate và check JsonElement có phải là object hợp lệ không
    /// </summary>
    public static void ValidateObjectData(JsonElement dataElement, string contextMessage)
    {
        if (dataElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException($"Dữ liệu {contextMessage} không đúng định dạng (không phải object).");
        }
    }
}
