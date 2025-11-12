using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Payment.PayOS;

public static class PayOSApiClientHelpers
{
    /// <summary>
    /// Deep sort object with optional array sorting
    /// </summary>
    private static Dictionary<string, object?> DeepSortObject(Dictionary<string, object?> obj, bool sortArrays = false)
    {
        var sorted = new Dictionary<string, object?>();
        
        foreach (var key in obj.Keys.OrderBy(k => k))
        {
            var value = obj[key];
            
            if (value is JsonElement jsonElement)
            {
                value = JsonElementToObject(jsonElement);
            }
            
            if (value is List<object> list)
            {
                if (sortArrays)
                {
                    sorted[key] = list
                        .Select(item => item is Dictionary<string, object?> dict ? DeepSortObject(dict, sortArrays) : item)
                        .OrderBy(item => JsonSerializer.Serialize(item))
                        .ToList();
                }
                else
                {
                    sorted[key] = list
                        .Select(item => item is Dictionary<string, object?> dict ? DeepSortObject(dict, sortArrays) : item)
                        .ToList();
                }
            }
            else if (value is Dictionary<string, object?> dict)
            {
                sorted[key] = DeepSortObject(dict, sortArrays);
            }
            else
            {
                sorted[key] = value;
            }
        }
        
        return sorted;
    }
    
    /// <summary>
    /// Convert JsonElement to appropriate C# object
    /// </summary>
    private static object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(JsonElementToObject)
                .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
    
    /// <summary>
    /// Create HMAC-SHA256 signature for PayOS API
    /// </summary>
    /// <param name="secretKey">Secret key for HMAC signature generation</param>
    /// <param name="data">Data object to be signed</param>
    /// <param name="encodeUri">Whether to URL encode keys and values (default: true)</param>
    /// <param name="sortArrays">Whether to sort array elements (default: false)</param>
    /// <returns>HMAC signature in hexadecimal format</returns>
    public static string CreateSignature(string secretKey, Dictionary<string, object?> data, bool encodeUri = true, bool sortArrays = false)
    {
        var sortedData = DeepSortObject(data, sortArrays);
        
        var queryString = string.Join("&", sortedData.Keys.OrderBy(k => k).Select(key =>
        {
            var value = sortedData[key];
            string valueStr;
            
            // Handle arrays by JSON stringify them
            if (value is List<object> || value is object[] || value is IEnumerable<object>)
            {
                valueStr = JsonSerializer.Serialize(value);
            }
            // Handle nested objects
            else if (value is Dictionary<string, object?>)
            {
                valueStr = JsonSerializer.Serialize(value);
            }
            // Handle null/undefined values
            else if (value == null)
            {
                valueStr = "";
            }
            else
            {
                valueStr = value.ToString() ?? "";
            }
            
            // Conditionally URL encode the key and value based on options
            if (encodeUri)
            {
                return $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(valueStr)}";
            }
            return $"{key}={valueStr}";
        }));
        
        // Create HMAC-SHA256 signature
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
    
    /// <summary>
    /// Generate a unique idempotency key (UUID)
    /// </summary>
    public static string GenerateIdempotencyKey()
    {
        return Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Generate a unique reference ID with timestamp
    /// </summary>
    public static string GenerateReferenceId()
    {
        return $"payout_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
