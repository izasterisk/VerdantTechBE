using BLL.DTO.MediaLink;
using BLL.DTO.VendorCertificate;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IVendorCertificateService
    {
        Task<List<VendorCertificateResponseDTO>> GetAllByVendorIdAsync( ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO?> GetByIdAsync( ulong id, CancellationToken ct = default);
        Task<List<VendorCertificateResponseDTO>> CreateAsync( VendorCertificateCreateDto dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO> UpdateAsync( VendorCertificateUpdateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, List<string> removedCertificates, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO> ChangeStatusAsync( VendorCertificateChangeStatusDTO dto, CancellationToken ct = default);
       
    }
}
