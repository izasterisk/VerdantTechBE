using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory?> GetProductCategoryByIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetAllProductCategoryAsync( CancellationToken cancellationToken = default);
        Task<ProductCategory> CreateProductCategoryAsync(ProductCategory ProductCategory, CancellationToken cancellationToken = default);
        Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory ProductCategory, CancellationToken cancellationToken = default);
    }
}
