using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductCertificate;
using DAL.Data;
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
        Task<PagedResponse<ProductCertificateResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<PagedResponse<ProductCertificateResponseDTO>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
        Task<ProductCertificateResponseDTO?> GetByIdAsync(ulong productCertificateId, CancellationToken ct = default);
        Task<bool> ChangeStatusAsync(ProductCertificateChangeStatusDTO dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductCertificateResponseDTO>> CreateAsync(ProductCertificateCreateDTO dto, List<MediaLinkItemDTO> addCertificates, CancellationToken ct = default);
        Task<ProductCertificateResponseDTO> UpdateAsync(ProductCertificateUpdateDTO dto, List<MediaLinkItemDTO> addCertificates, List<string> removedCertificates, CancellationToken ct = default);
       
    }
}
