using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<ProductCategory> CreateProductCategoryAsync(ProductCategory ProductCategory, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.CreateAsync(ProductCategory, cancellationToken);
        }

        public async Task<List<ProductCategory>> GetAllProductCategoryAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProductCategories
                .Where(pc => pc.IsActive)  // Thử bỏ điều kiện này
                .ToListAsync(cancellationToken);
        }


        public async Task<ProductCategory?> GetProductCategoryByIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.GetAsync(pc => pc.Id == categoryId, useNoTracking, cancellationToken);
        }

        public async Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory ProductCategory, CancellationToken cancellationToken = default)
        {
            return await _productCategoryRepository.UpdateAsync(ProductCategory, cancellationToken);
        }
    }
}
