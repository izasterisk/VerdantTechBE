using BLL.DTO.Order;
using DAL.Data;
using Microsoft.Extensions.Caching.Memory;

namespace BLL.Helpers.Order;

public class OrderHelper
{
    /// <summary>
    /// Dictionary ánh xạ các trạng thái đơn hàng hiện tại với các trạng thái hợp lệ có thể chuyển đến.
    /// </summary>
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedOrderStatusTransitions = new()
    {
        [OrderStatus.Pending] = new() { OrderStatus.Paid, OrderStatus.Cancelled },
        [OrderStatus.Paid] = new() { OrderStatus.Cancelled, OrderStatus.Processing },
        [OrderStatus.Processing] = new() { OrderStatus.Cancelled, OrderStatus.Shipped },
        [OrderStatus.Shipped] = new() { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = new() { OrderStatus.Cancelled, OrderStatus.Refunded },
        [OrderStatus.Cancelled] = new() { OrderStatus.Refunded },
        [OrderStatus.Refunded] = new() { } 
    };

    /// <summary>
    /// Kiểm tra xem có thể chuyển từ trạng thái hiện tại sang trạng thái mới hay không.
    /// </summary>
    /// <param name="currentStatus">Trạng thái hiện tại của đơn hàng</param>
    /// <param name="newStatus">Trạng thái mới muốn chuyển đến</param>
    /// <returns>True nếu chuyển đổi hợp lệ, False nếu không hợp lệ</returns>
    public static bool IsValidOrderStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (currentStatus == newStatus)
            return true;
        return AllowedOrderStatusTransitions.TryGetValue(currentStatus, out var allowedStatuses) 
               && allowedStatuses.Contains(newStatus);
    }

    /// <summary>
    /// Lấy danh sách các trạng thái hợp lệ có thể chuyển đến từ trạng thái hiện tại.
    /// </summary>
    /// <param name="currentStatus">Trạng thái hiện tại của đơn hàng</param>
    /// <returns>Chuỗi mô tả các trạng thái hợp lệ</returns>
    public static string GetAllowedOrderStatusTransitions(OrderStatus currentStatus)
    {
        if (AllowedOrderStatusTransitions.TryGetValue(currentStatus, out var allowedStatuses) && allowedStatuses.Any())
            return string.Join(", ", allowedStatuses);
        return "Không có trạng thái hợp lệ (trạng thái cuối)";
    }

    /// <summary>
    /// Validate việc chuyển đổi trạng thái và throw exception nếu không hợp lệ.
    /// </summary>
    /// <param name="currentStatus">Trạng thái hiện tại của đơn hàng</param>
    /// <param name="newStatus">Trạng thái mới muốn chuyển đến</param>
    /// <exception cref="InvalidOperationException">Khi chuyển đổi không hợp lệ</exception>
    public static void ValidateOrderStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (!IsValidOrderStatusTransition(currentStatus, newStatus))
        {
            throw new InvalidOperationException(
                $"Không thể chuyển trạng thái đơn hàng từ '{currentStatus}' sang '{newStatus}'. " +
                $"Các trạng thái hợp lệ từ '{currentStatus}': {GetAllowedOrderStatusTransitions(currentStatus)}"
            );
        }
    }

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

    /// <summary>
    /// Convert các giá trị decimal sang int cho API courier (GoShip).
    /// Làm tròn lên bất kỳ phần thập phân nào (5.1 → 6).
    /// </summary>
    public static (int width, int height, int length, int weight, int cod) ConvertDimensionsToInt(
        decimal width, decimal height, decimal length, decimal weight, decimal cod)
    {
        return (
            (int)Math.Ceiling((double)width),
            (int)Math.Ceiling((double)height),
            (int)Math.Ceiling((double)length),
            (int)Math.Ceiling((double)weight),
            (int)Math.Ceiling((double)cod)
        );
    }
}