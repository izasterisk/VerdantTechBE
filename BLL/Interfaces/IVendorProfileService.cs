using BLL.DTO.MediaLink;
using BLL.DTO.VendorProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IVendorProfileService
    {
        Task<VendorProfileResponseDTO> CreateAsync(VendorProfileCreateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default);
        Task<VendorProfileResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<List<VendorProfileResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<VendorProfileResponseDTO> UpdateAsync(ulong id, VendorProfileUpdateDTO dto, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
    }
}