using DAL.Data;
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
        // READS (paged)
        Task<(IReadOnlyList<ProductCertificate> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<ProductCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<(IReadOnlyList<ProductCertificate> Items, int Total)> GetByProductAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);

        // CREATE / UPDATE (kèm MediaLink PDF)
        Task<ProductCertificate> CreateAsync(
            ProductCertificate certificate,
            IEnumerable<MediaLink>? addCertificateFiles,
            CancellationToken ct = default);

        Task<ProductCertificate> UpdateAsync(
            ProductCertificate certificate,
            IEnumerable<MediaLink>? addCertificateFiles,
            IEnumerable<string>? removeCertificatePublicIds,
            CancellationToken ct = default);

        // STATUS / DELETE
        Task<bool> ChangeStatusAsync(
            ulong id,
            ProductCertificateStatus status,
            string? rejectionReason,
            ulong? verifiedBy,
            DateTime? verifiedAt,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
    }
}
