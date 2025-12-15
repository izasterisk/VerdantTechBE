using DAL.Data.Models;

namespace DAL.IRepository;

public interface IProductUpdateRequestRepository
{
    Task<(ProductSnapshot, ulong requestId)> CreateProductUpdateRequestWithTransactionAsync(ProductSnapshot productSnapshot,
        List<MediaLink> images, ProductUpdateRequest productUpdateRequest, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken);
    Task<bool> IsThereAnyPendingRequestAsync(ulong productId, CancellationToken cancellationToken);
    Task<List<MediaLink>> GetAllImagesByProductSnapshotIdAsync(ulong productSnapshotId, CancellationToken cancellationToken);
    Task<List<MediaLink>> GetAllImagesByProductIdAsync(ulong productId, CancellationToken cancellationToken);
    Task<ProductUpdateRequest> GetProductUpdateRequestAsync(ulong productUpdateRequestId, CancellationToken cancellationToken);
}