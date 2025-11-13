using BLL.DTO;
using BLL.DTO.ProductReview;
using BLL.DTO.MediaLink;

namespace BLL.Interfaces;

public interface IProductReviewService
{
    Task<ProductReviewResponseDTO> CreateProductReviewAsync(ulong customerId, ProductReviewCreateDTO dto, List<MediaLinkItemDTO>? images, CancellationToken ct = default);
    Task<ProductReviewResponseDTO?> GetProductReviewByIdAsync(ulong id, CancellationToken ct = default);
    Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByOrderIdAsync(ulong orderId, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByCustomerIdAsync(ulong customerId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductReviewResponseDTO> UpdateProductReviewAsync(ulong customerId, ulong reviewId, ProductReviewUpdateDTO dto, CancellationToken ct = default);
    Task<bool> DeleteProductReviewAsync(ulong customerId, ulong reviewId, CancellationToken ct = default);
}

