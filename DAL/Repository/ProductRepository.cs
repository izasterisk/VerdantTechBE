using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IRepository<Product> _productRepository;
        private readonly VerdantTechDbContext _context;
        public ProductRepository(IRepository<Product> productRepository, VerdantTechDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<Product> CreateProductAsync(Product Product, CancellationToken cancellationToken = default)
        {
            return await _productRepository.CreateAsync(Product, cancellationToken);
        }

        public async Task<List<Product>> GetAllProductAsync(CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetAllAsync(cancellationToken);
        }

        public async Task<List<Product>> GetAllProductByCategoryIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetAllByFilterAsync(c => c.CategoryId == categoryId, useNoTracking, cancellationToken);
        }

        public async Task<Product?> GetProductByIdAsync(ulong categoryId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _productRepository.GetAsync(p => p.Id == categoryId, useNoTracking, cancellationToken);
        }

        public async Task<Product> UpdateProductAsync(Product Product, CancellationToken cancellationToken = default)
        {
            return await _productRepository.UpdateAsync(Product, cancellationToken);
        }
    }
}
