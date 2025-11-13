using DAL.Data.Models;

namespace DAL.IRepository;

public interface IProductReviewRepository
{
    Task<ProductReview?> GetProductReviewByIdAsync(ulong id, bool useNoTracking = true, CancellationToken ct = default);
    Task<ProductReview?> GetProductReviewByProductOrderCustomerAsync(ulong productId, ulong orderId, ulong customerId, bool useNoTracking = true, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByOrderIdAsync(ulong orderId, int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductReview> Items, int Total)> GetProductReviewsByCustomerIdAsync(ulong customerId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductReview> CreateProductReviewWithTransactionAsync(ProductReview review, List<MediaLink>? mediaLinks, CancellationToken ct = default);
    Task<ProductReview> UpdateProductReviewAsync(ProductReview review, CancellationToken ct = default);
    Task<bool> DeleteProductReviewAsync(ulong id, CancellationToken ct = default);
    Task<decimal> CalculateProductRatingAverageAsync(ulong productId, CancellationToken ct = default);
    Task<List<MediaLink>> GetAllImagesByReviewIdAsync(ulong reviewId, CancellationToken ct = default);
}

