using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class ExportInventoryRepository : IExportInventoryRepository
{
    private readonly IRepository<ExportInventory> _exportInventoryRepository;
    private readonly VerdantTechDbContext _dbContext;
    
    public ExportInventoryRepository(VerdantTechDbContext context)
    {
        _exportInventoryRepository = new Repository<ExportInventory>(context);
        _dbContext = context;
    }
    
    public async Task CreateExportInventoryWithTransactionAsync(List<ExportInventory> exportInventories, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var exportInventory in exportInventories)
            {
                exportInventory.CreatedAt = DateTime.UtcNow;
                exportInventory.UpdatedAt = DateTime.UtcNow;
                await _exportInventoryRepository.CreateAsync(exportInventory, cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}