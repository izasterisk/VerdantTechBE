using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class EnvironmentalDataRepository : IEnvironmentalDataRepository
{
    private readonly IRepository<EnvironmentalDatum> _environmentalDataRepository;
    private readonly IRepository<FarmProfile> _farmProfileRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IFertilizerRepository _fertilizerRepository;
    private readonly IEnergyUsageRepository _energyUsageRepository;
    
    public EnvironmentalDataRepository(
        IRepository<EnvironmentalDatum> environmentalDataRepository, 
        IRepository<FarmProfile> farmProfileRepository, 
        VerdantTechDbContext dbContext,
        IFertilizerRepository fertilizerRepository,
        IEnergyUsageRepository energyUsageRepository)
    {
        _environmentalDataRepository = environmentalDataRepository;
        _farmProfileRepository = farmProfileRepository;
        _dbContext = dbContext;
        _fertilizerRepository = fertilizerRepository;
        _energyUsageRepository = energyUsageRepository;
    }

    public async Task<bool> GetEnvironmentDataByFarmIdAndDateRangeAsync(ulong farmProfileId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _environmentalDataRepository.AnyAsync(
            x => x.FarmProfileId == farmProfileId 
                 && x.MeasurementStartDate == startDate && x.MeasurementEndDate == endDate, cancellationToken);
    }
    
    public async Task<List<EnvironmentalDatum>> GetAllEnvironmentDataByFarmId(ulong id, CancellationToken cancellationToken = default)
    {
        return await _environmentalDataRepository.GetAllWithRelationsByFilterAsync(
            e => e.FarmProfileId == id,
            true,
            query => query.Include(e => e.Fertilizer)
                .Include(e => e.EnergyUsage),
            cancellationToken);
    }
    
    public async Task<EnvironmentalDatum?> GetEnvironmentDataById(ulong id, CancellationToken cancellationToken = default)
    {
        return await _environmentalDataRepository.GetWithRelationsAsync(
            e => e.Id == id,
            true,
            query => query.Include(e => e.Fertilizer)
                .Include(e => e.EnergyUsage),
            cancellationToken);
    }
    
    public async Task<EnvironmentalDatum> CreateEnvironmentalDataWithTransactionAsync(EnvironmentalDatum environmentalDatum, Fertilizer fertilizer, EnergyUsage energyUsage, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            environmentalDatum.CreatedAt = DateTime.UtcNow;
            environmentalDatum.UpdatedAt = DateTime.UtcNow;
            var createdEnvironmentalData = await _environmentalDataRepository.CreateAsync(environmentalDatum, cancellationToken);
            
            fertilizer.EnvironmentalDataId = createdEnvironmentalData.Id;
            fertilizer.CreatedAt = DateTime.UtcNow;
            fertilizer.UpdatedAt = DateTime.UtcNow;
            await _fertilizerRepository.CreateFertilizerAsync(fertilizer, cancellationToken);
            
            energyUsage.EnvironmentalDataId = createdEnvironmentalData.Id;
            energyUsage.CreatedAt = DateTime.UtcNow;
            energyUsage.UpdatedAt = DateTime.UtcNow;
            await _energyUsageRepository.CreateEnergyUsageAsync(energyUsage, cancellationToken);
            
            var completeEnvironmentalData = await GetEnvironmentDataById(createdEnvironmentalData.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return completeEnvironmentalData!;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<bool> DeleteEnvironmentalDataByIdWithTransactionAsync(ulong id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var environmentalDatum = await GetEnvironmentDataById(id, cancellationToken);
            if (environmentalDatum == null)
            {
                return false;
            }
            if (environmentalDatum.Fertilizer != null)
            {
                await _fertilizerRepository.DeleteFertilizerAsync(environmentalDatum.Fertilizer, cancellationToken);
            }
            if (environmentalDatum.EnergyUsage != null)
            {
                await _energyUsageRepository.DeleteEnergyUsageAsync(environmentalDatum.EnergyUsage, cancellationToken);
            }
            var result = await _environmentalDataRepository.DeleteAsync(environmentalDatum, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}