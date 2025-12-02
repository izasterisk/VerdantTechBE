using BLL.DTO.MediaLink;
using BLL.DTO.VendorProfile;
using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{ 
    public interface IVendorProfileService
    {
       Task<VendorProfileResponseDTO> CreateAsync( VendorProfileCreateDTO dto, IEnumerable<MediaLink>? addVendorCertificateFiles, CancellationToken ct = default);
       Task<VendorProfileResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
       Task<VendorProfileResponseDTO?> GetByUserIdAsync(ulong userId, CancellationToken ct = default);
       Task<List<VendorProfileResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
       Task UpdateAsync(VendorProfileUpdateDTO dto, CancellationToken ct = default);
       Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task SoftDeleteAccountAsync(ulong vendorProfileId, CancellationToken ct = default);
       Task ApproveAsync(VendorProfileApproveDTO dto, CancellationToken ct = default);
       Task RejectAsync(VendorProfileRejectDTO dto, CancellationToken ct = default);
    }
}