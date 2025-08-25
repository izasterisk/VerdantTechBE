using DAL.Data.Models;

namespace DAL.IRepository;

public interface IVendorProfilesRepository
{
    Task<VendorProfile> CreateVendorProfileWithTransactionAsync(VendorProfile vendorProfile);
}