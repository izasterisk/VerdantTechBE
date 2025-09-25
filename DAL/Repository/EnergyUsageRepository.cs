using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class EnergyUsageRepository : IEnergyUsageRepository
{
    private readonly IRepository<EnergyUsage> _energyUsageRepository;
    
    public EnergyUsageRepository(IRepository<EnergyUsage> energyUsageRepository)
    {
        _energyUsageRepository = energyUsageRepository;
    }
    
    public async Task<EnergyUsage> CreateEnergyUsageAsync(EnergyUsage energyUsage, CancellationToken cancellationToken = default)
    {
        energyUsage.CreatedAt = DateTime.UtcNow;
        energyUsage.UpdatedAt = DateTime.UtcNow;
        return await _energyUsageRepository.CreateAsync(energyUsage, cancellationToken);
    }
    
    public async Task<EnergyUsage> UpdateEnergyUsageAsync(EnergyUsage energyUsage, CancellationToken cancellationToken = default)
    {
        energyUsage.UpdatedAt = DateTime.UtcNow;
        return await _energyUsageRepository.UpdateAsync(energyUsage, cancellationToken);
    }
}