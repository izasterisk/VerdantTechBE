using DAL.Data.Models;

namespace DAL.IRepository;

public interface IEnvironmentalDataRepository
{
    Task<EnvironmentalDatum> CreateEnvironmentalDataWithSoilDataAsync(ulong farmProfileId, ulong customerId, DateOnly measurementStartDate, DateOnly measurementEndDate, decimal sandPct, decimal siltPct, decimal clayPct, decimal phh2o, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> GetByFarmAndDateRangeAsync(ulong farmProfileId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}