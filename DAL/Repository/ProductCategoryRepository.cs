using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
namespace DAL.Repository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly VerdantTechDbContext _dbContext;

        public ProductCategoryRepository(IRepository<ProductCategory> repository, VerdantTechDbContext dbContext)
        {
            _productCategoryRepository = repository;
            _dbContext = dbContext;
        }

        public async Task<ProductCategory> CreateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.CreateAsync(productCategory, cancellationToken);
        }
        
        public async Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.UpdateAsync(productCategory, cancellationToken);
        }

        public async Task<List<ProductCategory>> GetAllProductCategoryAsync(CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.GetAllWithRelationsAsync(
                q => q
                    .AsNoTracking().Where(pc => pc.IsActive)
                    .Include(pc => pc.Parent), cancellationToken
            );
        }

        public async Task<ProductCategory?> GetProductCategoryByIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.GetWithRelationsAsync(
                pc => pc.Id == categoryId,
                useNoTracking,
                q => q.Include(pc => pc.Parent),
                cancellationToken
            );
        }
        public async Task<bool> IsCategoryNameNotUniqueAsync(string name, CancellationToken cancellationToken = default)
        {
            var upperName = name.ToUpper();

            return await _productCategoryRepository.AnyAsync(pc => pc.Name.ToUpper() == upperName, cancellationToken);
        }
        
        public async Task<bool> IsParentIdExistsAsync(ulong parentId, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.AnyAsync(pc => pc.Id == parentId, cancellationToken);
        }

        public async Task<bool> IsCategoryAlreadyAFatherAsync(ulong id, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.AnyAsync(pc => pc.ParentId == id, cancellationToken);
        }
    }
}
