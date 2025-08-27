using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repository;

public class SustainabilityCertificationsRepository : ISustainabilityCertificationsRepository
{
    private readonly IRepository<SustainabilityCertification> _sustainabilityCertificationRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public SustainabilityCertificationsRepository(VerdantTechDbContext context)
    {
        _sustainabilityCertificationRepository = new Repository<SustainabilityCertification>(context);
        _dbContext = context;
    }
    
    public async Task<SustainabilityCertification> CreateSustainabilityCertificationWithTransactionAsync(SustainabilityCertification sustainabilityCertification)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            sustainabilityCertification.IsActive = true;
            sustainabilityCertification.CreatedAt = DateTime.Now;
            sustainabilityCertification.UpdatedAt = DateTime.Now;
            var createdSustainabilityCertification = await _sustainabilityCertificationRepository.CreateAsync(sustainabilityCertification);
            await transaction.CommitAsync();
            return createdSustainabilityCertification;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<SustainabilityCertification> UpdateSustainabilityCertificationWithTransactionAsync(SustainabilityCertification sustainabilityCertification)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            sustainabilityCertification.UpdatedAt = DateTime.Now;
            var updatedSustainabilityCertification = await _sustainabilityCertificationRepository.UpdateAsync(sustainabilityCertification);
            await transaction.CommitAsync();
            return updatedSustainabilityCertification;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<SustainabilityCertification?> GetSustainabilityCertificationByIdAsync(ulong id)
    {
        return await _sustainabilityCertificationRepository.GetAsync(sc => sc.Id == id && sc.IsActive == true);
    }
    
    public async Task<(List<SustainabilityCertification>, int totalCount)> GetAllSustainabilityCertificationsAsync(int page, int pageSize, String? category = null)
    {
        Expression<Func<SustainabilityCertification, bool>> filter = u => u.IsActive == true;
        
        // Apply category filter if provided
        if (!string.IsNullOrEmpty(category))
        {
            if (Enum.TryParse<SustainabilityCertificationCategory>(category, true, out var categoryEnum))
            {
                filter = u => u.IsActive == true && u.Category == categoryEnum;
            }
        }

        return await _sustainabilityCertificationRepository.GetPaginatedAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(u => u.UpdatedAt)
        );
    }

    public Task<List<SustainabilityCertificationCategory>> GetAllCategoriesAsync()
    {
        var categories = Enum.GetValues<SustainabilityCertificationCategory>()
            .OrderBy(c => c.ToString())
            .ToList();

        return Task.FromResult(categories);
    }
    
    public async Task<bool> CheckCodeExistsAsync(string code)
    {
        return await _sustainabilityCertificationRepository.AnyAsync(sc => sc.Code.ToUpper() == code.ToUpper());
    }
}