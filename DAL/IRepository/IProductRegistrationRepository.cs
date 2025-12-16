using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;

namespace DAL.Repositories;

public interface IProductRegistrationRepository
{
    // READS (có phân trang)
    Task<(IReadOnlyList<ProductRegistration> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);

    Task<ProductRegistration?> GetByIdAsync(ulong id, CancellationToken ct = default);

    Task<(IReadOnlyList<ProductRegistration> Items, int Total)> GetByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);

    // CREATE / UPDATE
    Task<ProductRegistration> CreateAsync(ProductRegistration registration, IEnumerable<MediaLink>? productImages, IEnumerable<MediaLink>? certificateImages, CancellationToken ct = default);

    Task<ProductRegistration> CreateForImportAsync(ProductRegistration registration, CancellationToken ct = default);

    Task<ProductRegistration> UpdateAsync( ProductRegistration registration, IEnumerable<MediaLink>? addProductImages, IEnumerable<MediaLink>? addCertificateImages, IEnumerable<string>? removeImagePublicIds, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default);

    // STATUS / DELETE
    Task<bool> ChangeStatusAsync( ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, DateTime? approvedAt, CancellationToken ct = default);

    Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
}
