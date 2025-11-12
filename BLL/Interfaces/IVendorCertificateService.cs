using BLL.DTO.MediaLink;
using BLL.DTO.VendorCertificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IVendorCertificateService
    {
        Task<IReadOnlyList<VendorCertificateResponseDTO>> CreateAsync(VendorCertificateCreateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO> UpdateAsync(VendorCertificateUpdateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, List<string> removedCertificates, CancellationToken ct = default);
        //Task<VendorCertificateResponseDTO> UpdateAsync(ulong id, VendorCertificateUpdateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, List<string> removedCertificates, CancellationToken ct = default);
        //Task<VendorCertificateResponseDTO> CreateAsync(VendorCertificateCreateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<List<VendorCertificateResponseDTO>> GetAllByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<VendorCertificateResponseDTO> ChangeStatusAsync(VendorCertificateChangeStatusDTO dto, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
    }
}
