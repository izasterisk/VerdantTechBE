using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

    public async Task<(ProductSnapshot, ulong requestId)> CreateProductUpdateRequestWithTransactionAsync
    (ProductSnapshot productSnapshot, List<MediaLink> imagesToAdd, List<MediaLink> imagesToKeep, 
        ProductUpdateRequest productUpdateRequest, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            productSnapshot.CreatedAt = DateTime.UtcNow;
            productSnapshot.UpdatedAt = DateTime.UtcNow;
            var snapshot = await _productSnapshotRepository.CreateAsync(productSnapshot, cancellationToken);

            if (imagesToKeep.Count > 0)
            {
                foreach (var image in imagesToKeep)
                {
                    image.OwnerId = snapshot.Id;
                    image.CreatedAt = DateTime.UtcNow;
                    image.UpdatedAt = DateTime.UtcNow;
                }
                await _mediaLinkRepository.CreateBulkAsync(imagesToKeep, cancellationToken);
            }
            if (imagesToAdd.Count > 0)
            {
                foreach (var image in imagesToAdd)
                {
                    image.OwnerId = snapshot.Id;
                    image.CreatedAt = DateTime.UtcNow;
                    image.UpdatedAt = DateTime.UtcNow;
                }
                await _mediaLinkRepository.CreateBulkAsync(imagesToAdd, cancellationToken);
            }
            
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
    
    public async Task ApproveProductUpdateRequestAsync
        (ProductUpdateRequest request, ProductSnapshot productSnapshot, List<MediaLink> productSnapshotImages, 
            Product productUpdate, List<MediaLink> productUpdateImages, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _productUpdateRequestRepository.UpdateAsync(request, cancellationToken);
            
            productSnapshot.CreatedAt = DateTime.UtcNow;
            productSnapshot.UpdatedAt = DateTime.UtcNow;
            var snapshot = await _productSnapshotRepository.CreateAsync(productSnapshot, cancellationToken);

            foreach (var productSnapshotImage in productSnapshotImages)
            {
                productSnapshotImage.UpdatedAt = DateTime.UtcNow;
                productSnapshotImage.OwnerId = snapshot.Id;
                productSnapshotImage.OwnerType = MediaOwnerType.ProductSnapshot;
            }
            await _mediaLinkRepository.BulkUpdateAsync(productSnapshotImages, cancellationToken);
            
            productUpdate.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(productUpdate, cancellationToken);
            
            foreach (var productUpdateImage in productUpdateImages)
            {
                productUpdateImage.Id = 0;
                productUpdateImage.OwnerType = MediaOwnerType.Products;
                productUpdateImage.OwnerId = productUpdate.Id;
                productUpdateImage.UpdatedAt = DateTime.UtcNow;
                productUpdateImage.CreatedAt = DateTime.UtcNow;
            }
            await _mediaLinkRepository.CreateBulkAsync(productUpdateImages, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task DeleteProductUpdateRequestAsync(ProductUpdateRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _dbContext.MediaLinks
                .Where(ml => ml.OwnerType == MediaOwnerType.ProductSnapshot && ml.OwnerId == request.ProductSnapshotId)
                .ExecuteDeleteAsync(cancellationToken);

            _dbContext.ProductUpdateRequests.Remove(request);
            
            var productSnapshot = await _dbContext.ProductSnapshots
                .FirstOrDefaultAsync(ps => ps.Id == request.ProductSnapshotId, cancellationToken)
                ?? throw new KeyNotFoundException($"Không tìm thấy bản sao sản phẩm với Id: {request.ProductSnapshotId}.");
            
            _dbContext.ProductSnapshots.Remove(productSnapshot);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException($"Không thể xóa yêu cầu cập nhật sản phẩm với Id: {request.Id}. Lỗi: {ex.Message}", ex);
        }
    }
    
    public async Task RejectProductUpdateRequestAsync(ProductUpdateRequest request, CancellationToken cancellationToken)
        => await _productUpdateRequestRepository.UpdateAsync(request, cancellationToken);
    
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

    public async Task<ProductUpdateRequest> GetProductUpdateRequestWithRelationsByIdAsync(ulong productUpdateRequestId, CancellationToken cancellationToken)
    {
        return await _productUpdateRequestRepository.GetWithRelationsAsync
                   (pur => pur.Id == productUpdateRequestId, true, 
                       q => q.Include(r => r.ProductSnapshot)
                           .Include(r => r.Product)
                           .Include(r => r.ProcessedByUser),
                       cancellationToken)
               ?? throw new KeyNotFoundException($"Không tìm thấy yêu cầu cập nhật sản phẩm với Id: {productUpdateRequestId}.");
    }
    
    public async Task<ProductUpdateRequest> GetProductUpdateRequestByIdAsync(ulong productUpdateRequestId, CancellationToken cancellationToken)
        => await _productUpdateRequestRepository.GetAsync(p => p.Id == productUpdateRequestId, true, cancellationToken)
              ?? throw new KeyNotFoundException($"Không tìm thấy yêu cầu cập nhật sản phẩm với Id: {productUpdateRequestId}.");
    
    public async Task<List<ProductUpdateRequest>> GetAllProductUpdateRequestsByVendorUserIdAsync(ulong userId, CancellationToken cancellationToken)
        => await _productUpdateRequestRepository.GetAllWithRelationsByFilterAsync(p => p.Product.VendorId == userId, true,
            q => q.Include(r => r.Product)
                .Include(r => r.ProductSnapshot)
                .Include(r => r.ProcessedByUser),
            cancellationToken);
    
    public async Task<(List<ProductUpdateRequest>, int totalCount)> GetAllProductUpdateRequestsAsync(int page, int pageSize, ProductRegistrationStatus? status = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<ProductUpdateRequest, bool>> filter = pur => true;
        
        // Apply status filter
        if (status.HasValue)
        {
            filter = pur => pur.Status == status.Value;
        }

        return await _productUpdateRequestRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(pur => pur.UpdatedAt),
            includeFunc: 
            query => query.Include(pur => pur.ProductSnapshot)
                                       .Include(pur => pur.ProcessedByUser),
            cancellationToken
        );
    }
    
    public async Task<(List<ProductSnapshot>, int totalCount)> GetAllProductHistoriesAsync(ulong productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        Expression<Func<ProductSnapshot, bool>> filter = ps => ps.ProductId == productId && ps.SnapshotType == ProductSnapshotType.History;

        return await _productSnapshotRepository.GetPaginatedAsync(
            page,
            pageSize,
            filter,
            useNoTracking: true,
            orderBy: query => query.OrderByDescending(ps => ps.CreatedAt),
            cancellationToken
        );
    }
}