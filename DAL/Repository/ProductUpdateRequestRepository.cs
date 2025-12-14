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
    private readonly VerdantTechDbContext _dbContext;
    
    public ProductUpdateRequestRepository(IRepository<ProductCategory> productCategoryRepository, IRepository<Product> productRepository,
        IRepository<ProductSnapshot> productSnapshotRepository, IRepository<ProductUpdateRequest> productUpdateRequestRepository,
        IRepository<User> userRepository, VerdantTechDbContext dbContext)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _productSnapshotRepository = productSnapshotRepository;
        _productUpdateRequestRepository = productUpdateRequestRepository;
        _userRepository = userRepository;
        _dbContext = dbContext;
    }
    
    public async Task<Product> GetProductByIdAsync(ulong productId, CancellationToken cancellationToken)
    {
        return await _productRepository.GetAsync(p => p.Id == productId, true,
            cancellationToken) 
               ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm với Id: {productId}.");
    }
}