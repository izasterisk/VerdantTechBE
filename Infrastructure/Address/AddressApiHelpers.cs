using System.Text.Json;
using BLL.DTO.Address;

namespace Infrastructure.Address;

/// <summary>
/// Helper methods cho GoShip Address API
/// Xử lý parsing, validation và error handling cho các API calls
/// </summary>
public static class AddressApiHelpers
{
    /// <summary>
    /// Parse GoShip API response và trích xuất data array
    /// </summary>
    /// <param name="jsonContent">JSON response từ GoShip API</param>
    /// <param name="contextMessage">Message context cho error (vd: "tỉnh/thành phố", "quận/huyện")</param>
    /// <param name="resourceId">ID của resource (optional, dùng cho chi tiết error)</param>
    /// <returns>JsonElement chứa data array để caller tự mapping</returns>
    /// <exception cref="InvalidOperationException">Khi response không hợp lệ hoặc có lỗi</exception>
    public static JsonElement ParseGoshipResponse(
        string jsonContent, 
        string contextMessage,
        string? resourceId = null)
    {
        ValidateJsonContent(jsonContent, contextMessage, resourceId);

        try
        {
            var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            // Validate GoShip response structure
            ValidateGoshipResponseStructure(root, contextMessage, resourceId);

            // Extract and validate data array
            var dataElement = root.GetProperty("data");
            ValidateDataArray(dataElement, contextMessage, resourceId);

            return dataElement;
        }
        catch (JsonException ex)
        {
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException(
                $"Không thể parse JSON response từ GoShip API khi lấy {errorContext}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate nội dung JSON response không rỗng và không phải HTML
    /// </summary>
    private static void ValidateJsonContent(string jsonContent, string contextMessage, string? resourceId)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException($"Response rỗng khi lấy dữ liệu {errorContext}.");
        }

        // Check if response is HTML (error page) instead of JSON
        if (jsonContent.TrimStart().StartsWith("<"))
        {
            var errorContext = resourceId != null 
                ? $"API endpoint không khả dụng hoặc mã {contextMessage} {resourceId} không đúng."
                : $"API endpoint không khả dụng hoặc URL không đúng khi lấy {contextMessage}.";
            throw new InvalidOperationException(errorContext);
        }
    }

    /// <summary>
    /// Validate cấu trúc response của GoShip API (kiểm tra code và data property)
    /// </summary>
    private static void ValidateGoshipResponseStructure(JsonElement root, string contextMessage, string? resourceId)
    {
        // Check response code (GoShip trả về code 200 khi thành công)
        if (root.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() != 200)
        {
            var message = root.TryGetProperty("message", out var msgElement) 
                ? msgElement.GetString() 
                : "Unknown error";
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException($"GoShip API error khi lấy {errorContext}: {message}");
        }

        // Check data property exists
        if (!root.TryGetProperty("data", out _))
        {
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException($"Response không có trường 'data' khi lấy {errorContext}.");
        }
    }

    /// <summary>
    /// Validate data array từ GoShip response
    /// </summary>
    private static void ValidateDataArray(JsonElement dataElement, string contextMessage, string? resourceId)
    {
        // Handle null data (error case)
        if (dataElement.ValueKind == JsonValueKind.Null)
        {
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException($"Dữ liệu {errorContext} không tồn tại (data is null).");
        }

        // Validate data is array
        if (dataElement.ValueKind != JsonValueKind.Array)
        {
            var errorContext = resourceId != null 
                ? $"{contextMessage} (ID: {resourceId})" 
                : contextMessage;
            throw new InvalidOperationException($"Dữ liệu {errorContext} không đúng định dạng (không phải array).");
        }
    }

    /// <summary>
    /// Parse và filter danh sách districts từ GoShip V1 API response theo province code
    /// Sử dụng streaming approach với Utf8JsonReader để tiết kiệm bộ nhớ khi xử lý large JSON (27500+ items)
    /// </summary>
    /// <param name="jsonContent">JSON response từ GoShip API</param>
    /// <param name="provinceCode">Province code để filter (ví dụ: "100000")</param>
    /// <returns>Danh sách districts thuộc province code đã chọn</returns>
    /// <exception cref="InvalidOperationException">Khi JSON không hợp lệ</exception>
    public static List<CourierDistrictResponseDTO> ParseAndFilterDistricts(string jsonContent, string provinceCode)
    {
        var districts = new List<CourierDistrictResponseDTO>();
        
        // Parse JSON using Utf8JsonReader for memory efficiency
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
        var reader = new System.Text.Json.Utf8JsonReader(bytes);
        
        // Navigate to root object
        if (!reader.Read() || reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
        {
            throw new InvalidOperationException("JSON không đúng định dạng (không bắt đầu bằng object).");
        }
        
        // Find "data" array
        bool foundData = false;
        while (reader.Read())
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.PropertyName && 
                reader.GetString() == "data")
            {
                foundData = true;
                reader.Read(); // Move to array start
                
                if (reader.TokenType != System.Text.Json.JsonTokenType.StartArray)
                {
                    throw new InvalidOperationException("Trường 'data' không phải là array.");
                }
                
                // Stream through array elements
                while (reader.Read() && reader.TokenType != System.Text.Json.JsonTokenType.EndArray)
                {
                    if (reader.TokenType == System.Text.Json.JsonTokenType.StartObject)
                    {
                        // Parse individual object
                        using var doc = System.Text.Json.JsonDocument.ParseValue(ref reader);
                        var element = doc.RootElement;
                        
                        // Check if city_code matches
                        if (element.TryGetProperty("city_code", out var cityCodeProp))
                        {
                            var cityCode = cityCodeProp.GetString();
                            if (cityCode == provinceCode)
                            {
                                // Map to DTO
                                districts.Add(new CourierDistrictResponseDTO
                                {
                                    DistrictCode = element.GetProperty("code").GetString() ?? string.Empty,
                                    Name = element.GetProperty("name").GetString() ?? string.Empty
                                });
                            }
                        }
                    }
                }
                break;
            }
        }
        
        if (!foundData)
        {
            throw new InvalidOperationException("Response không có trường 'data' khi lấy quận/huyện.");
        }
        
        return districts;
    }

    /// <summary>
    /// Xử lý exception chung cho các API calls (HttpRequestException, TaskCanceledException, etc.)
    /// </summary>
    /// <param name="ex">Exception được throw</param>
    /// <param name="contextMessage">Context message (vd: "tỉnh/thành phố")</param>
    /// <param name="resourceId">ID của resource (optional)</param>
    /// <exception cref="TimeoutException">Khi request bị timeout</exception>
    /// <exception cref="InvalidOperationException">Khi có lỗi kết nối hoặc lỗi khác</exception>
    public static void HandleApiException(Exception ex, string contextMessage, string? resourceId = null)
    {
        var errorContext = resourceId != null 
            ? $"{contextMessage} (ID: {resourceId})" 
            : contextMessage;

        switch (ex)
        {
            case TaskCanceledException:
                throw new TimeoutException("Server GoShip hiện đang quá tải, vui lòng thử lại sau.");
            
            case HttpRequestException httpEx:
                throw new InvalidOperationException($"Không thể kết nối đến server GoShip: {httpEx.Message}");
            
            case InvalidOperationException invalidOpEx:
                throw invalidOpEx; // Re-throw custom exceptions
            
            default:
                throw new InvalidOperationException(
                    $"Lỗi không xác định khi lấy danh sách {errorContext}: {ex.Message}");
        }
    }
}