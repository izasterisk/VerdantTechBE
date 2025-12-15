using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

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

    public async Task<(ProductSnapshot, ulong requestId)> CreateProductUpdateRequestWithTransactionAsync(ProductSnapshot productSnapshot, 
        List<MediaLink> images, ProductUpdateRequest productUpdateRequest, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
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
            var createdRequest = await _productUpdateRequestRepository.CreateAsync(productUpdateRequest, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return (snapshot, createdRequest.Id);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
    
    public async Task<List<MediaLink>> GetAllImagesByProductSnapshotIdAsync(ulong productSnapshotId, CancellationToken cancellationToken)
    {
        return await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.ProductSnapshot && ml.OwnerId == productSnapshotId)
            .OrderBy(ml => ml.SortOrder)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<MediaLink>> GetAllImagesByProductIdAsync(ulong productId, CancellationToken cancellationToken)
    {
        return await _dbContext.MediaLinks
            .AsNoTracking()
            .Where(ml => ml.OwnerType == MediaOwnerType.Products && ml.OwnerId == productId)
            .OrderBy(ml => ml.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductUpdateRequest> GetProductUpdateRequestAsync(ulong productUpdateRequestId, CancellationToken cancellationToken)
    {
        return await _productUpdateRequestRepository.GetWithRelationsAsync
                   (pur => pur.Id == productUpdateRequestId, true, 
                       q => q.Include(r => r.ProductSnapshot)
                           .Include(r => r.Product)
                           .Include(r => r.ProcessedByUser),
                       cancellationToken)
               ?? throw new KeyNotFoundException($"Không tìm thấy yêu cầu cập nhật sản phẩm với Id: {productUpdateRequestId}.");
    }
}