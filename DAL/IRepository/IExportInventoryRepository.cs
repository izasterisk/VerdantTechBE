using DAL.Data.Models;

namespace DAL.IRepository;

public interface IExportInventoryRepository
{
    Task CreateExportInventoryWithTransactionAsync(List<ExportInventory> exportInventories,
        CancellationToken cancellationToken = default);
}