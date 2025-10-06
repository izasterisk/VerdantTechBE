using AutoMapper;
using BLL.DTO.Cart;
using DAL.IRepository;

namespace BLL.Helpers.Cart;

public static class CartHelper
{
    /// <summary>
    /// Load hình ảnh cho tất cả cart items trong 1 lần để tối ưu performance (tránh N+1 query problem)
    /// </summary>
    public static async Task PopulateCartItemsImagesAsync(
        List<CartItemDTO> cartItems, 
        ICartRepository cartRepository, 
        IMapper mapper,
        CancellationToken cancellationToken = default)
    {
        if (cartItems == null || !cartItems.Any()) return;

        var productIds = cartItems.Select(item => item.ProductId).Distinct().ToList();

        // Load tất cả hình ảnh của tất cả sản phẩm trong 1 query duy nhất
        var allImages = await cartRepository.GetAllProductImagesForMultipleProducts(productIds, cancellationToken);

        // Group theo ProductId để phân bổ cho từng cart item
        var imagesByProduct = allImages.GroupBy(img => img.OwnerId)
                                       .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var item in cartItems)
        {
            if (imagesByProduct.TryGetValue(item.ProductId, out var images) && images.Any())
            {
                item.Images = mapper.Map<List<ImagesDTO>>(images);
            }
            else
            {
                // Đảm bảo không null để tránh NullReferenceException
                item.Images = new List<ImagesDTO>();
            }
        }
    }
}