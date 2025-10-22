using BLL.DTO.Order;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Helpers.Order;

public class OrderHelper
{
    public static (decimal length, decimal width, decimal height) CalculatePackageDimensions(decimal length, 
        decimal width, decimal height, decimal nextLength, decimal nextWidth, decimal nextHeight, int quantity)
    {
        var option1 = (length + nextLength * quantity, Math.Max(width, nextWidth),
            Math.Max(height, nextHeight));
        var option2 = (Math.Max(length, nextLength), width + nextWidth * quantity,
            Math.Max(height, nextHeight));
        var option3 = (Math.Max(length, nextLength), Math.Max(width, nextWidth),
            height + nextHeight * quantity);

        decimal volume1 = option1.Item1 * option1.Item2 * option1.Item3;
        decimal volume2 = option2.Item1 * option2.Item2 * option2.Item3;
        decimal volume3 = option3.Item1 * option3.Item2 * option3.Item3;

        if (volume1 <= volume2 && volume1 <= volume3)
            return option1;
        if (volume2 <= volume1 && volume2 <= volume3)
            return option2;
        return option3;
    }

    /// <summary>
    /// Generate cache key cho OrderPreview.
    /// </summary>
    public static string GenerateOrderPreviewCacheKey(Guid orderPreviewId)
    {
        return $"OrderPreview_{orderPreviewId}";
    }

    /// <summary>
    /// Lưu OrderPreviewResponseDTO vào cache với thời gian hết hạn 10 phút.
    /// </summary>
    public static void CacheOrderPreview(IMemoryCache memoryCache, Guid orderPreviewId, OrderPreviewResponseDTO data)
    {
        var cacheKey = GenerateOrderPreviewCacheKey(orderPreviewId);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        memoryCache.Set(cacheKey, data, cacheOptions);
    }

    /// <summary>
    /// Lấy OrderPreviewResponseDTO từ cache thông qua OrderPreviewId.
    /// </summary>
    /// <returns>OrderPreviewResponseDTO nếu tìm thấy, null nếu không tìm thấy hoặc đã hết hạn.</returns>
    public static OrderPreviewResponseDTO? GetOrderPreviewFromCache(IMemoryCache memoryCache, Guid orderPreviewId)
    {
        var cacheKey = GenerateOrderPreviewCacheKey(orderPreviewId);
        if (memoryCache.TryGetValue(cacheKey, out OrderPreviewResponseDTO? cachedPreview))
        {
            return cachedPreview;
        }
        return null;
    }

    /// <summary>
    /// Xóa OrderPreviewResponseDTO khỏi cache sau khi đã tạo order thành công.
    /// Điều này đảm bảo preview không thể được sử dụng lại để tạo nhiều order.
    /// </summary>
    public static void RemoveOrderPreviewFromCache(IMemoryCache memoryCache, Guid orderPreviewId)
    {
        var cacheKey = GenerateOrderPreviewCacheKey(orderPreviewId);
        memoryCache.Remove(cacheKey);
    }
}