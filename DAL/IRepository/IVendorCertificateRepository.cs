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

        Task<VendorCertificate> CreateAsync(ulong id, VendorCertificate vendorcertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, CancellationToken ct = default);
        Task<VendorCertificate> UpdateAsync(ulong id, VendorCertificate vendorcertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default);
        Task<VendorCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<List<VendorCertificate>> GetAllByVendorIdAsync(ulong vendorId,int page, int pageSize, CancellationToken ct = default);
        Task DeleteCertificateAsync(VendorCertificate vendorcertificatey, CancellationToken ct = default);


       
    }
}
