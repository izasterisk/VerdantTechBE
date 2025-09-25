using DAL.Data.Models;

namespace DAL.IRepository;

public interface IEnvironmentalDataRepository
{
    Task<bool> GetEnvironmentDataByFarmIdAndDateRangeAsync(ulong farmProfileId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<List<EnvironmentalDatum>> GetAllEnvironmentDataByFarmId(ulong id, CancellationToken cancellationToken = default);
    Task<EnvironmentalDatum?> GetEnvironmentDataById(ulong id, CancellationToken cancellationToken = default);
    Task<EnvironmentalDatum> CreateEnvironmentalDataWithTransactionAsync(EnvironmentalDatum environmentalDatum, Fertilizer fertilizer, EnergyUsage energyUsage, CancellationToken cancellationToken = default);
    Task<bool> DeleteEnvironmentalDataByIdWithTransactionAsync(ulong id, CancellationToken cancellationToken = default);
}