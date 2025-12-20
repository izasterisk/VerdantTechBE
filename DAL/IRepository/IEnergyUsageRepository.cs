using DAL.Data.Models;

namespace DAL.IRepository;

public interface IEnergyUsageRepository
{
    Task<EnergyUsage> CreateEnergyUsageAsync(EnergyUsage energyUsage, CancellationToken cancellationToken = default);
    Task<bool> DeleteEnergyUsageAsync(EnergyUsage energyUsage, CancellationToken cancellationToken = default);
}