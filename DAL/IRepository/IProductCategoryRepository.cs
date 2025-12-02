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
        Task<bool> IsCategoryNameNotUniqueAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> IsCategoryAlreadySonsAsync(ulong parentId, CancellationToken cancellationToken = default);
        Task<ProductCategory?> GetProductCategoryByIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<ProductCategory> Items, int Total)> GetAllProductCategoryAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<ProductCategory> CreateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task<ProductCategory> UpdateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    }
}
