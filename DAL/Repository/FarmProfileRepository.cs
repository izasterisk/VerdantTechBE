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

    public async Task<FarmProfile?> GetFarmProfileByFarmIdAsync(ulong farmId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        => await _farmProfileRepository.GetAsync(f => f.Id == farmId, useNoTracking, cancellationToken);

    public async Task<List<FarmProfile>> GetAllFarmProfilesByUserIdAsync(ulong userId, bool useNoTracking = true, CancellationToken cancellationToken = default)
        => await _farmProfileRepository.GetAllByFilterAsync
            (f => f.UserId == userId && f.Status != FarmProfileStatus.Deleted, useNoTracking, cancellationToken);
    
    public Task<FarmProfile> CreateAsync(FarmProfile entity, CancellationToken cancellationToken = default) => _farmProfileRepository.CreateAsync(entity, cancellationToken);

    public Task<FarmProfile> UpdateAsync(FarmProfile entity, CancellationToken cancellationToken = default) => _farmProfileRepository.UpdateAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(FarmProfile entity, CancellationToken cancellationToken = default) => _farmProfileRepository.DeleteAsync(entity, cancellationToken);
}
