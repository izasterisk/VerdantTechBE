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
    private readonly IRepository<Product> _productRepository; 
    
    public ExportInventoryRepository(IRepository<ExportInventory> exportInventoryRepository, IRepository<BatchInventory> batchInventoryRepository,
        VerdantTechDbContext dbContext, IRepository<ProductSerial> productSerialRepository,
        IRepository<Product> productRepository)
    {
        _exportInventoryRepository = exportInventoryRepository;
        _batchInventoryRepository = batchInventoryRepository;
        _dbContext = dbContext;
        _productSerialRepository = productSerialRepository;
        _productRepository = productRepository;
    }
    
    public async Task<List<ulong>> CreateExportForOrderWithTransactionAsync(List<ExportInventory> exportInventories, List<ProductSerial> s, CancellationToken cancellationToken = default)
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
                    serial.Status = ProductSerialStatus.Sold;
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
    
    public async Task<List<ulong>> CreateExportForExportWithTransactionAsync(List<ExportInventory> exportInventories, Dictionary<ulong, int> productQuantities, List<ProductSerial> s, CancellationToken cancellationToken = default)
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
            if (productQuantities.Count > 0)
            {
                foreach (var productQuantity in productQuantities)
                {
                    var product = await _productRepository.GetAsync(p => p.Id == productQuantity.Key, true, cancellationToken)
                        ?? throw new KeyNotFoundException($"Sản phẩm với ID {productQuantity.Key} không tồn tại.");
                    if(product.StockQuantity < productQuantity.Value)
                        throw new InvalidOperationException($"Sản phẩm với ID {product.Id} không đủ số lượng trong kho để xuất. Số lượng hiện có: {product.StockQuantity}, Số lượng yêu cầu: {productQuantity.Value}.");
                    product.StockQuantity -= productQuantity.Value;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product, cancellationToken);
                }
            }
            if (s.Count > 0)
            {
                foreach (var serial in s)
                {
                    serial.Status = ProductSerialStatus.Adjustment;
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
        var exportQuantity = await _dbContext.ExportInventories
            .AsNoTracking()
            .Where(e => e.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase))
            .SumAsync(e => e.Quantity, cancellationToken);
        var import = await _batchInventoryRepository.GetAsync(
            b => b.LotNumber.Equals(lotNumber, StringComparison.OrdinalIgnoreCase),
            true, cancellationToken) ?? 
                throw new KeyNotFoundException("Lô hàng không tồn tại trong kho.");
        return import.Quantity - exportQuantity;
    }
    
    public async Task<List<(string LotNumber, int RemainingQuantity)>> GetNumberOfProductsLeftInInventoryThruLotNumbersAsync(List<string> lotNumbers, CancellationToken cancellationToken = default)
    {
        var imports = _dbContext.BatchInventories
            .AsNoTracking()
            .Where(b => lotNumbers.Contains(b.LotNumber))
            .Select(b => new { b.LotNumber, Quantity = b.Quantity });

        var exports = _dbContext.ExportInventories
            .AsNoTracking()
            .Where(e => lotNumbers.Contains(e.LotNumber))
            .Select(e => new { e.LotNumber, Quantity = -e.Quantity });

        var result = await imports.Concat(exports)
            .GroupBy(x => x.LotNumber)
            .Select(g => new 
            {
                LotNumber = g.Key,
                Remaining = g.Sum(x => x.Quantity)
            })
            .Where(x => x.Remaining > 0)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.LotNumber, x.Remaining)).ToList();
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
    
    public async Task<List<string>> GetAllLotNumbersByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        var lotNumbers = await _dbContext.BatchInventories
            .AsNoTracking()
            .Where(b => b.ProductId == productId)
            .Select(b => b.LotNumber)
            .Distinct()
            .OrderBy(lot => lot)
            .ToListAsync(cancellationToken);
        return lotNumbers;
    }
    
    public async Task<List<(string LotNumber, string SerialNumber)>> GetSerialNumbersWithLotNumbersByProductIdAsync(ulong productId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.ProductSerials
            .AsNoTracking()
            .Where(ps => ps.ProductId == productId && ps.Status == ProductSerialStatus.Stock)
            .Include(ps => ps.BatchInventory)
            .Select(ps => new 
            {
                LotNumber = ps.BatchInventory.LotNumber,
                SerialNumber = ps.SerialNumber
            })
            .OrderBy(x => x.LotNumber)
            .ThenBy(x => x.SerialNumber)
            .ToListAsync(cancellationToken);
        
        return result.Select(x => (x.LotNumber, x.SerialNumber)).ToList();
    }
}