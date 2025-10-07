using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IProductRegistrationRepository
    {
        Task <ProductRegistration> CreateProductAsync(ProductRegistration entity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductRegistration>> GetProductRegistrationByVendorIdAsync(ulong vendorId, bool useNoTracking = true, CancellationToken cancellationToken = default);
    }
}
