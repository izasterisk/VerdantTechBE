using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IProductCertificateRepository
    {
        Task<ProductCertificate> CreateProductCertificateAsync(ProductCertificate ProductCertificate, CancellationToken cancellationToken = default);
        Task<ProductCertificate?> GetProductCertificateByIdAsync(ulong ProductCertificateId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductCertificate>> GetAllProductCertificatesByProductIdAsync(ulong ProductId, CancellationToken cancellationToken = default);
        Task<ProductCertificate> UpdateProductCertificateWithTransactionAsync(ProductCertificate ProductCertificate, CancellationToken cancellationToken = default);
    }
}
