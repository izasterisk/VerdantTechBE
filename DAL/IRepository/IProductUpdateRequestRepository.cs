using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface IProductUpdateRequestRepository
{
    Task<(ProductSnapshot, ulong requestId)> CreateProductUpdateRequestWithTransactionAsync(ProductSnapshot productSnapshot,
        List<MediaLink> imagesToAdd, List<MediaLink> imagesToKeep, ProductUpdateRequest productUpdateRequest, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken);
    Task<bool> IsThereAnyPendingRequestAsync(ulong productId, CancellationToken cancellationToken);
    Task<List<MediaLink>> GetAllImagesByProductSnapshotIdAsync(ulong productSnapshotId, CancellationToken cancellationToken);
    Task<List<MediaLink>> GetAllImagesByProductIdAsync(ulong productId, CancellationToken cancellationToken);
    Task<ProductUpdateRequest> GetProductUpdateRequestWithRelationsByIdAsync(ulong productUpdateRequestId, CancellationToken cancellationToken);
    Task<(List<ProductUpdateRequest>, int totalCount)> GetAllProductUpdateRequestsAsync(int page, int pageSize, ProductRegistrationStatus? status = null, CancellationToken cancellationToken = default);
    Task ApproveProductUpdateRequestAsync
    (ProductUpdateRequest request, ProductSnapshot productSnapshot, List<MediaLink> productSnapshotImages,
        Product productUpdate, List<MediaLink> productUpdateImages, CancellationToken cancellationToken);
    Task RejectProductUpdateRequestAsync(ProductUpdateRequest request, CancellationToken cancellationToken);
    Task<ProductUpdateRequest> GetProductUpdateRequestByIdAsync(ulong productUpdateRequestId, CancellationToken cancellationToken);
}