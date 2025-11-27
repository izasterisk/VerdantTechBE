using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DAL.Repository;

public class ExportInventoryRepository : IExportInventoryRepository
{
    private readonly IRepository<ExportInventory> _exportInventoryRepository;
    private readonly IRepository<BatchInventory> _batchInventoryRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<ProductSerial> _productSerialRepository; 
    
    public ExportInventoryRepository(IRepository<ExportInventory> exportInventoryRepository,
        IRepository<BatchInventory> batchInventoryRepository,
        VerdantTechDbContext dbContext,
        IRepository<ProductSerial> productSerialRepository)
    {
        _exportInventoryRepository = exportInventoryRepository;
        _batchInventoryRepository = batchInventoryRepository;
        _dbContext = dbContext;
        _productSerialRepository = productSerialRepository;
    }
    
    public async Task<List<ulong>> CreateExportNUpdateProductSerialsWithTransactionAsync(List<ExportInventory> exportInventories, ProductSerialStatus x, List<ProductSerial> s, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var exportIds = new List<ulong>();
            foreach (var exportInventory in exportInventories)
            {
                exportInventory.CreatedAt = DateTime.UtcNow;
                exportInventory.UpdatedAt = DateTime.UtcNow;
                var export = await _exportInventoryRepository.CreateAsync(exportInventory, cancellationToken);
                exportIds.Add(export.Id);
            }
            if (s.Count > 0)
            {
                foreach (var serial in s)
                {
                    serial.Status = x;
                    serial.UpdatedAt = DateTime.UtcNow;
                    await _productSerialRepository.UpdateAsync(serial, cancellationToken);
                }
            }
            await transaction.CommitAsync(cancellationToken);
            return exportIds;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<List<ExportInventory>> GetListedExportInventoriesByIdsAsync(List<ulong> ids, CancellationToken cancellationToken = default)
    {
        var exportInventories = new List<ExportInventory>();
        foreach (var id in ids)
        {
            var exportInventory = await _exportInventoryRepository.GetWithRelationsAsync(e => e.Id == id, true, 
                func => func.Include(e => e.CreatedByNavigation)
                    .Include(e => e.Product)
                    .Include(e => e.ProductSerial),
                cancellationToken)
                    ?? throw new KeyNotFoundException($"Không tìm thấy đơn xuất kho với ID {id}.");
            exportInventories.Add(exportInventory);
        }
        return exportInventories;
    }
    
    public async Task<ExportInventory?> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _exportInventoryRepository.GetWithRelationsAsync(
            e => e.Id == id, 
            true, 
            func => func.Include(e => e.CreatedByNavigation)
                .Include(e => e.Product)
                .Include(e => e.ProductSerial),
            cancellationToken);
    }
    
    public async Task<int> GetNumberOfProductLeftInInventoryThruLotNumberAsync(string lotNumber, CancellationToken cancellationToken = default)
    {
        var export = await _dbContext.ExportInventories
            .AsNoTracking()
            .Where(e => e.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase))
            .CountAsync(cancellationToken);
        var import = await _batchInventoryRepository.GetAsync(
            b => b.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase),
            true, cancellationToken) ?? 
                throw new KeyNotFoundException("Lô hàng không tồn tại trong kho.");
        return import.Quantity - export;
    }
    
    public async Task<(List<ExportInventory>, int totalCount)> GetAllExportInventoriesAsync(int page, int pageSize, string? movementType = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<ExportInventory, bool>> filter = e => true;
        
        // Apply MovementType filter if provided
        if (!string.IsNullOrEmpty(movementType))
        {
            if (Enum.TryParse<MovementType>(movementType, true, out var parsedMovementType))
            {
                filter = e => e.MovementType == parsedMovementType;
            }
        }

        return await _exportInventoryRepository.GetPaginatedWithRelationsAsync(
            page, 
            pageSize, 
            filter, 
            useNoTracking: true, 
            orderBy: query => query.OrderByDescending(e => e.CreatedAt),
            includeFunc: query => query
                .Include(e => e.CreatedByNavigation)
                .Include(e => e.Product)
                .Include(e => e.ProductSerial),
            cancellationToken
        );
    }
}