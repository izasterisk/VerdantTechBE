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
        Task<VendorProfile?> GetByUserIdAsync(ulong vendorId, CancellationToken ct = default);
        Task<List<VendorProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task UpdateAsync(VendorProfile vendorProfile, CancellationToken ct = default);
        //Task DeleteAsync(VendorProfile vendorProfile, CancellationToken ct = default); 
        Task HardDeleteVendorAsync(ulong vendorProfileId, CancellationToken ct);
        Task SoftDeleteVendorAsync(ulong vendorProfileId, CancellationToken ct = default);
        Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);
        Task<bool> ExistsByBusinessRegistrationNumberAsync(string brn, CancellationToken ct);
        Task<bool> ExistsByTaxCodeAsync(string taxCode, CancellationToken ct);
        Task<List<VendorProfile>> GetAllVerifiedVendorProfilesAsync(CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetAllVendorTransactionsAsync(CancellationToken cancellationToken = default);
        Task<List<Product>> GetAllProductsToBanAsync(List<ulong> vendorIds, CancellationToken cancellationToken = default);
        Task<List<ProductSnapshot>> GetAllProductsToUnBanAsync(List<ulong> vendorIds, CancellationToken cancellationToken = default);
    }
}