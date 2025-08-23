using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class VendorProfilesRepository : IVendorProfilesRepository
{
    private readonly IRepository<VendorProfile> _vendorProfileRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public VendorProfilesRepository(VerdantTechDbContext context)
    {
        _vendorProfileRepository = new Repository<VendorProfile>(context);
        _dbContext = context;
    }
    
    public async Task<VendorProfile> CreateVendorProfileWithTransactionAsync(VendorProfile vendorProfile)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            vendorProfile.CreatedAt = DateTime.Now;
            vendorProfile.UpdatedAt = DateTime.Now;
            var createdVendorProfile = await _vendorProfileRepository.CreateAsync(vendorProfile);
            await transaction.CommitAsync();
            return createdVendorProfile;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<VendorProfile> UpdateVendorProfileWithTransactionAsync(VendorProfile vendorProfile)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            vendorProfile.UpdatedAt = DateTime.Now;
            var updatedVendorProfile = await _vendorProfileRepository.UpdateAsync(vendorProfile);
            await transaction.CommitAsync();
            return updatedVendorProfile;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<VendorProfile?> GetVendorProfileByUserIdAsync(ulong userId)
    {
        return await _vendorProfileRepository.GetAsync(vp => vp.UserId == userId);
    }
}