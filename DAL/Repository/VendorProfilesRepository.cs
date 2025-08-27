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
    
    
}