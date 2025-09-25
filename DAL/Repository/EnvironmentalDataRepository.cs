using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class EnvironmentalDataRepository : IEnvironmentalDataRepository
{
    private readonly IRepository<EnvironmentalDatum> _environmentalDataRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public EnvironmentalDataRepository(VerdantTechDbContext context)
    {
        _environmentalDataRepository = new Repository<EnvironmentalDatum>(context);
        _dbContext = context;
    }

    public async Task<EnvironmentalDatum> CreateEnvironmentalDataWithTransactionAsync(EnvironmentalDatum environmentalData, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            environmentalData.CreatedAt = DateTime.UtcNow;
            environmentalData.UpdatedAt = DateTime.UtcNow;
            
            var createdData = await _environmentalDataRepository.CreateAsync(environmentalData, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return createdData;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<EnvironmentalDatum> CreateEnvironmentalDataWithSoilDataAsync(ulong farmProfileId, ulong customerId, DateOnly measurementStartDate, DateOnly measurementEndDate, decimal sandPct, decimal siltPct, decimal clayPct, decimal phh2o, string? notes = null, CancellationToken cancellationToken = default)
    {
        var environmentalData = new EnvironmentalDatum
        {
            FarmProfileId = farmProfileId,
            CustomerId = customerId,
            MeasurementStartDate = measurementStartDate,
            MeasurementEndDate = measurementEndDate,
            SandPct = sandPct,
            SiltPct = siltPct,
            ClayPct = clayPct,
            Phh2o = phh2o,
            Notes = notes
        };
        
        return await CreateEnvironmentalDataWithTransactionAsync(environmentalData, cancellationToken);
    }

    public async Task<EnvironmentalDatum> CreateEnvironmentalDataWithSoilAndWeatherDataAsync(ulong farmProfileId, ulong customerId, DateOnly measurementStartDate, DateOnly measurementEndDate, decimal sandPct, decimal siltPct, decimal clayPct, decimal phh2o, decimal precipitationSum, decimal et0FaoEvapotranspiration, string? notes = null, CancellationToken cancellationToken = default)
    {
        var environmentalData = new EnvironmentalDatum
        {
            FarmProfileId = farmProfileId,
            CustomerId = customerId,
            MeasurementStartDate = measurementStartDate,
            MeasurementEndDate = measurementEndDate,
            SandPct = sandPct,
            SiltPct = siltPct,
            ClayPct = clayPct,
            Phh2o = phh2o,
            PrecipitationSum = precipitationSum,
            Et0FaoEvapotranspiration = et0FaoEvapotranspiration,
            Notes = notes
        };
        
        return await CreateEnvironmentalDataWithTransactionAsync(environmentalData, cancellationToken);
    }

    public async Task<bool> GetByFarmAndDateRangeAsync(ulong farmProfileId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        await _environmentalDataRepository.AnyAsync(
            x => x.FarmProfileId == farmProfileId 
                 && x.MeasurementStartDate == startDate 
                 && x.MeasurementEndDate == endDate,
            cancellationToken);
}