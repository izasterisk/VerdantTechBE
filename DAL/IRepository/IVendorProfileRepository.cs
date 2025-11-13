using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IVendorProfileRepository
    {
        Task<VendorProfile> CreateAsync(VendorProfile vendorProfile, IEnumerable<MediaLink>? addVendorCertificateFiles, CancellationToken ct = default);
        Task<VendorProfile?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<List<VendorProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task UpdateAsync(VendorProfile vendorProfile, CancellationToken ct = default);
        Task DeleteAsync(VendorProfile vendorProfile, CancellationToken ct = default);

    }
}