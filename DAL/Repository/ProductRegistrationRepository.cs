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
    public class ProductRegistrationRepository : IProductRegistrationRepository
    {
        private readonly VerdantTechDbContext _context;
        private readonly IRepository<ProductRegistration> _repository;
        public ProductRegistrationRepository( VerdantTechDbContext context, IRepository<ProductRegistration> repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<ProductRegistration> CreateProductAsync(ProductRegistration entity, CancellationToken cancellationToken = default)
        {
            return await _repository.CreateAsync(entity, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductRegistration>> GetProductRegistrationByVendorIdAsync(ulong vendorId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllByFilterAsync(_repository => _repository.VendorId == vendorId, useNoTracking, cancellationToken);
        }

        public async Task<ProductRegistration> UpdateProductRegistrationAsync(ProductRegistration entity, CancellationToken cancellationToken = default)
        {
            return await _repository.UpdateAsync(entity, cancellationToken);
       }
        public async Task<ProductRegistration?> GetProductRegistrationByIdAsync(ulong id, bool useNoTracking = true, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAsync(_repository => _repository.Id == id, useNoTracking, cancellationToken);
        }
    }
}
