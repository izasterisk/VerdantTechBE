using BLL.DTO.ProductCertificate;
using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProductCertificateService
    {
        Task<ProductCertificateResponseDTO> CreateProductCertificateAsync(ProductCertificateCreateDTO productCertificate, CancellationToken cancellationToken = default);
        Task<ProductCertificateResponseDTO?> GetProductCertificateByIdAsync(ulong productCertificateId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductCertificateResponseDTO?>> GetAllProductCertificatesByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
        Task<ProductCertificateResponseDTO> UpdateProductCertificateAsync(ProductCertificateUpdateDTO productCertificate, CancellationToken cancellationToken = default);
    }
}
