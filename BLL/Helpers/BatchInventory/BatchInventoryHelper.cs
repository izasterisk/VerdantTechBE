using System.Security.Cryptography;
using System.Text;

namespace BLL.Helpers.BatchInventoryHelper;

public static class BatchInventoryHelper
{
    /// <summary>
    /// Tự động tạo SKU duy nhất cho batch inventory
    /// Format: SKU-{ProductId}-{Timestamp}-{RandomSuffix}
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <returns>SKU được tạo tự động</returns>
    public static string GenerateSku(ulong productId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = GenerateRandomSuffix(4);
        return $"SKU-{productId}-{timestamp}-{randomSuffix}";
    }

    /// <summary>
    /// Tạo random suffix để đảm bảo tính duy nhất
    /// </summary>
    /// <param name="length">Độ dài của suffix</param>
    /// <returns>Chuỗi random</returns>
    private static string GenerateRandomSuffix(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new StringBuilder(length);
        
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[length];
        rng.GetBytes(buffer);
        
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[buffer[i] % chars.Length]);
        }
        
        return result.ToString();
    }
}

