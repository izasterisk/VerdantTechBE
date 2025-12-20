using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class FertilizerRepository : IFertilizerRepository
{
    private readonly IRepository<Fertilizer> _fertilizerRepository;
    
    public FertilizerRepository(IRepository<Fertilizer> fertilizerRepository)
    {
        _fertilizerRepository = fertilizerRepository;
    }
    
    public async Task<Fertilizer> CreateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default)
    {
        fertilizer.CreatedAt = DateTime.UtcNow;
        fertilizer.UpdatedAt = DateTime.UtcNow;
        return await _fertilizerRepository.CreateAsync(fertilizer, cancellationToken);
    }
    
    public async Task<bool> DeleteFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default)
    {
        return await _fertilizerRepository.DeleteAsync(fertilizer, cancellationToken);
    }
}