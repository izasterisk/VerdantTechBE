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
    public class ProductCertificateRepository : IProductCertificateRepository
    {
        private readonly IRepository<ProductCertificate> _productCertificateRepository;
        private readonly VerdantTechDbContext _dbcontext;

        public ProductCertificateRepository(IRepository<ProductCertificate> productCertificateRepository, VerdantTechDbContext dbcontext)
        {
            _productCertificateRepository = productCertificateRepository;
            _dbcontext = dbcontext;
        }

        public async Task<ProductCertificate> CreateProductCertificateAsync(ProductCertificate ProductCertificate, CancellationToken cancellationToken = default)
        {
            return await _productCertificateRepository.CreateAsync(ProductCertificate, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductCertificate>> GetAllProductCertificatesByProductIdAsync(ulong ProductId, CancellationToken cancellationToken = default)
        {
            var list = await _productCertificateRepository.GetAllWithRelationsByFilterAsync(pc => pc.ProductId == ProductId, true);
            return list;
        }

        public async Task<ProductCertificate?> GetProductCertificateByIdAsync(ulong ProductCertificateId, CancellationToken cancellationToken = default)
        {
            return await _productCertificateRepository.GetAsync(pc => pc.Id == ProductCertificateId, true, cancellationToken);
        }

        public async Task<ProductCertificate> UpdateProductCertificateWithTransactionAsync(ProductCertificate ProductCertificate, CancellationToken cancellationToken = default)
        {
            return await _productCertificateRepository.UpdateAsync(ProductCertificate, cancellationToken);
        }
    }
}
