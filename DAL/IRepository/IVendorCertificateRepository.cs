using DAL.Data;
using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IVendorCertificateRepository
    {
        Task<VendorCertificate> CreateAsync( ulong vendorId, VendorCertificate vendorCertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, CancellationToken ct = default);
        Task<VendorCertificate> UpdateAsync( ulong id, VendorCertificate vendorCertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default);
        Task<VendorCertificate?> GetByIdAsync(ulong id,CancellationToken ct = default);
        Task<List<VendorCertificate>> GetAllByVendorIdAsync( ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task DeleteCertificateAsync( VendorCertificate vendorCertificate, CancellationToken ct = default);
        Task<VendorCertificate?> ApproveAsync( ulong id, VendorCertificateStatus status, ulong? verifiedByUserId, string? rejectionReason, CancellationToken ct = default);
        Task DeleteAllByVendorIdAsync(ulong vendorId, CancellationToken ct = default);
    }
}
