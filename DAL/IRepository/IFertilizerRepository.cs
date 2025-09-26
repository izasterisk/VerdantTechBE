using DAL.Data.Models;

namespace DAL.IRepository;

public interface IFertilizerRepository
{
    Task<Fertilizer> CreateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default);
    Task<bool> DeleteFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default);
    // Task<Fertilizer> UpdateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default);
}