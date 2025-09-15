using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class FarmProfileRepository : IFarmProfileRepository
{
    private readonly IRepository<FarmProfile> _farmProfileRepository;
    private readonly VerdantTechDbContext _context;

    public FarmProfileRepository(IRepository<FarmProfile> farmProfileRepository, VerdantTechDbContext context)
    {
        _farmProfileRepository = farmProfileRepository;
        _context = context;
    }

    public async Task<FarmProfile?> GetFarmProfileAsync(ulong farmId, bool useNoTracking = true)
        => await _farmProfileRepository.GetAsync(f => f.Id == farmId, useNoTracking);

    public async Task<List<FarmProfile>> GetAllFarmProfilesByUserAsync(ulong userId, bool useNoTracking = true)
        => await _farmProfileRepository.GetAllByFilterAsync(f => f.UserId == userId, useNoTracking);

    
    public Task<FarmProfile> CreateAsync(FarmProfile entity) => _farmProfileRepository.CreateAsync(entity);

    public Task<FarmProfile> UpdateAsync(FarmProfile entity) => _farmProfileRepository.UpdateAsync(entity);

    public Task<bool> DeleteAsync(FarmProfile entity) => _farmProfileRepository.DeleteAsync(entity);
}
