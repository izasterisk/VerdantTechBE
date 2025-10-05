using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IProductRepository
    {
        Task<Product?> GetProductByIdAsync(ulong Id, bool useNoTracking = true, CancellationToken cancellationToken = default);
        Task<List<Product>> GetAllProductAsync(CancellationToken cancellationToken = default);
        Task<List<Product>> GetAllProductByCategoryIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default);
        Task<Product> CreateProductAsync(Product Product, CancellationToken cancellationToken = default);
        Task<Product> UpdateProductAsync(Product Product, CancellationToken cancellationToken = default);
    }
}
