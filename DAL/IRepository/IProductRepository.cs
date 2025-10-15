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
        //Task<Product?> GetProductByIdAsync(ulong Id, bool useNoTracking = true, CancellationToken cancellationToken = default);
        //Task<List<Product>> GetAllProductAsync(CancellationToken cancellationToken = default);
        //Task<List<Product>> GetAllProductByCategoryIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default);
        //Task<Product> CreateProductAsync(Product Product, CancellationToken cancellationToken = default);
        //Task<Product> UpdateProductAsync(Product Product, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Product?> GetProductByIdAsync(ulong id, bool useNoTracking = true, CancellationToken ct = default);
        Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductByCategoryIdAsync(ulong categoryId, int page, int pageSize, CancellationToken ct = default);
        Task<(IReadOnlyList<Product> Items, int Total)> GetAllProductByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<Product> UpdateProductAsync(Product product, CancellationToken ct = default);
        Task<bool> UpdateEmissionAsync(ulong productId, decimal CommissionRate, CancellationToken ct = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);

    }
}
