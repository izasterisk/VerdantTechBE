using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class ExportInventoryRepository : IExportInventoryRepository
{
    private readonly IRepository<ExportInventory> _exportInventoryRepository;
    private readonly VerdantTechDbContext _dbContext;
    private readonly IRepository<ProductSerial> _productSerialRepository; 
    
    public ExportInventoryRepository(VerdantTechDbContext context, IRepository<ExportInventory> ExportInventory,
        IRepository<ProductSerial> ProductSerialRepository)
    {
        _dbContext = context;
        _exportInventoryRepository = ExportInventory;
        _productSerialRepository = ProductSerialRepository;
    }
    
    public async Task CreateExportNUpdateProductSerialsWithTransactionAsync(List<ExportInventory> exportInventories, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var exportInventory in exportInventories)
            {
                exportInventory.CreatedAt = DateTime.UtcNow;
                exportInventory.UpdatedAt = DateTime.UtcNow;
                var export = await _exportInventoryRepository.CreateAsync(exportInventory, cancellationToken);
                if (export.ProductSerialId != null)
                {
                    var productSerial = await _productSerialRepository.GetAsync(ps => ps.Id == export.ProductSerialId.Value, true, cancellationToken);
                    if (productSerial == null)
                        throw new Exception("Số sê-ri sản phẩm nhận vào không hợp lệ.");
                    productSerial.Status = ProductSerialStatus.Sold;
                    productSerial.UpdatedAt = DateTime.UtcNow;
                    await _productSerialRepository.UpdateAsync(productSerial, cancellationToken);
                }
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<ExportInventory?> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        return await _exportInventoryRepository.GetAsync(e => e.Id == id, true, cancellationToken);
    }
}