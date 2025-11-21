using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface IExportInventoryRepository
{
    Task<List<ulong>> CreateExportNUpdateProductSerialsWithTransactionAsync(List<ExportInventory> exportInventories, ProductSerialStatus x, CancellationToken cancellationToken = default);
    Task<List<ExportInventory>> GetListedExportInventoriesByIdsAsync(List<ulong> ids, CancellationToken cancellationToken = default);
    Task<ExportInventory?> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<(List<ExportInventory>, int totalCount)> GetAllExportInventoriesAsync(int page, int pageSize, string? movementType = null, CancellationToken cancellationToken = default);
}