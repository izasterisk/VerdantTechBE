using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class ProductUpdateRequestRepository : IProductUpdateRequestRepository
{
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductSnapshot> _productSnapshotRepository;
    private readonly IRepository<ProductUpdateRequest> _productUpdateRequestRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<MediaLink> _mediaLinkRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public ProductUpdateRequestRepository(IRepository<ProductCategory> productCategoryRepository, IRepository<Product> productRepository,
        IRepository<ProductSnapshot> productSnapshotRepository, IRepository<ProductUpdateRequest> productUpdateRequestRepository,
        IRepository<User> userRepository, IRepository<MediaLink> mediaLinkRepository,
        VerdantTechDbContext dbContext)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _productSnapshotRepository = productSnapshotRepository;
        _productUpdateRequestRepository = productUpdateRequestRepository;
        _userRepository = userRepository;
        _mediaLinkRepository = mediaLinkRepository;
        _dbContext = dbContext;
    }

    public async Task<ProductSnapshot> CreateProductUpdateRequestWithTransactionAsync(ProductSnapshot productSnapshot, 
        List<MediaLink> images, ProductUpdateRequest productUpdateRequest, CancellationToken cancellationToken)
    {
        productSnapshot.CreatedAt = DateTime.UtcNow;
        productSnapshot.UpdatedAt = DateTime.UtcNow;
        var snapshot = await _productSnapshotRepository.CreateAsync(productSnapshot, cancellationToken);

        foreach (var image in images)
        {
            image.OwnerId = snapshot.Id;
            image.CreatedAt = DateTime.UtcNow;
            image.UpdatedAt = DateTime.UtcNow;
        }
        await _mediaLinkRepository.CreateBulkAsync(images, cancellationToken);
        
        productUpdateRequest.ProductSnapshotId = snapshot.Id;
        productUpdateRequest.CreatedAt = DateTime.UtcNow;
        productUpdateRequest.UpdatedAt = DateTime.UtcNow;
        await _productUpdateRequestRepository.CreateAsync(productUpdateRequest, cancellationToken);

        return snapshot;
    }
    
    public async Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken)
    {
        return await _productRepository.GetAsync(p => p.Id == productId, true,
            cancellationToken) 
               ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm với Id: {productId}.");
    }
    
    public async Task<bool> IsThereAnyPendingRequestAsync(ulong productId, CancellationToken cancellationToken)
    {
        return await _productUpdateRequestRepository.AnyAsync(r => r.ProductId == productId && r.Status == ProductRegistrationStatus.Pending,
            cancellationToken);
    }
}