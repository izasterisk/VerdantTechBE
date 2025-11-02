using DAL.Data.Models;

namespace DAL.IRepository;

public interface IExportInventoryRepository
{
    Task CreateExportNUpdateProductSerialsWithTransactionAsync(List<ExportInventory> exportInventories, CancellationToken cancellationToken = default);
    Task<ExportInventory?> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default);
}