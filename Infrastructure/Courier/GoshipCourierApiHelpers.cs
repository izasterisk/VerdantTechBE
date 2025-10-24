using System.Text.Json;

namespace Infrastructure.Courier;

public class GoshipCourierApiHelpers
{
    /// <summary>
    /// Parse CommuneCode (string) sang ward_id (int) cho Goship V1 API
    /// </summary>
    /// <param name="communeCode">Mã xã/phường dạng string</param>
    /// <param name="addressType">Loại địa chỉ (người gửi/nhận) để hiển thị lỗi rõ ràng</param>
    /// <returns>Ward ID dạng int</returns>
    /// <exception cref="InvalidOperationException">Khi commune code không parse được sang int</exception>
    public static int ParseCommuneCodeToWardId(string? communeCode, string addressType)
    {
        if (string.IsNullOrWhiteSpace(communeCode))
        {
            throw new InvalidOperationException($"Mã xã/phường {addressType} không được để trống.");
        }
        
        if (!int.TryParse(communeCode, out var wardId))
        {
            throw new InvalidOperationException($"Mã xã/phường {addressType} không hợp lệ.");
        }
        
        return wardId;
    }
    
    /// <summary>
    /// Parse Goship Courier API response cho fees-list endpoint
    /// Response có nested structure: data.rates (không phải flat array như Address API)
    /// </summary>
    /// <param name="responseContent">JSON response từ Goship API</param>
    /// <returns>JsonElement chứa rates array</returns>
    /// <exception cref="InvalidOperationException">Khi response không hợp lệ</exception>
    public static JsonElement ParseFeesListResponse(string responseContent)
    {
        // Parse JSON response
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(responseContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể parse JSON response từ GoShip API khi lấy fees-list: {ex.Message}");
        }
        
        var root = document.RootElement;
        
        // Validate response code
        if (!root.TryGetProperty("code", out var codeElement) || codeElement.GetInt32() != 200)
        {
            var message = root.TryGetProperty("message", out var msgElement) 
                ? msgElement.GetString() 
                : "Unknown error";
            throw new InvalidOperationException($"GoShip API error khi lấy fees-list: {message}");
        }
        
        // Validate data exists
        if (!root.TryGetProperty("data", out var dataElement))
        {
            throw new InvalidOperationException("Response không có trường 'data' khi lấy fees-list.");
        }
        
        // V1 API có nested structure: data.rates
        if (!dataElement.TryGetProperty("rates", out var ratesElement))
        {
            throw new InvalidOperationException("Response không có trường 'rates' trong data.");
        }
        
        return ratesElement;
    }
    
    /// <summary>
    /// Parse Goship Courier API response cho shipments endpoint
    /// Accept cả code 200 và 402 là thành công (402 = tài khoản không đủ tiền nhưng đơn vẫn được tạo)
    /// </summary>
    /// <param name="responseContent">JSON response từ Goship API</param>
    /// <returns>Shipment ID (string)</returns>
    /// <exception cref="HttpRequestException">Khi response không hợp lệ hoặc có lỗi</exception>
    public static string ParseShipmentResponse(string responseContent)
    {
        // Validate response không phải HTML
        if (responseContent.TrimStart().StartsWith("<"))
        {
            throw new HttpRequestException("API endpoint không khả dụng hoặc định dạng request không đúng. Server trả về HTML thay vì JSON.");
        }
        
        // Parse JSON response
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(responseContent);
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException($"Không thể parse JSON response từ GoShip API khi tạo shipment: {ex.Message}");
        }
        
        var root = document.RootElement;
        
        // Kiểm tra code response - accept both 200 and 402 as success
        if (!root.TryGetProperty("code", out var codeElement))
        {
            throw new HttpRequestException("Response không có trường 'code'.");
        }
        
        var code = codeElement.GetInt32();
        if (code != 200 && code != 402)
        {
            var message = root.TryGetProperty("message", out var msgElement) 
                ? msgElement.GetString() 
                : "Unknown error";
                
            // Lấy chi tiết lỗi validation nếu có
            if (root.TryGetProperty("data", out var dataElement) && 
                dataElement.TryGetProperty("errors", out var errorsElement))
            {
                var errorDetails = errorsElement.GetRawText();
                throw new HttpRequestException($"GoShip API error khi tạo shipment: {message}. Chi tiết: {errorDetails}");
            }
            
            throw new HttpRequestException($"GoShip API error khi tạo shipment: {message}");
        }
        
        // Lấy id từ data.id
        if (!root.TryGetProperty("data", out var dataElement2) ||
            !dataElement2.TryGetProperty("id", out var idElement))
        {
            throw new HttpRequestException("Response không có trường 'data.id' khi tạo shipment.");
        }
        
        return idElement.GetString() ?? throw new HttpRequestException("Shipment ID là null.");
    }
}