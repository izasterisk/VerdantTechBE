using DAL.Data;
using DAL.Data.Models;

namespace DAL.IRepository;

public interface IExportInventoryRepository
{
    Task<List<ulong>> CreateExportForOrderWithTransactionAsync(List<ExportInventory> exportInventories, List<ProductSerial> s, CancellationToken cancellationToken = default);
    Task<List<ExportInventory>> GetListedExportInventoriesByIdsAsync(List<ulong> ids, CancellationToken cancellationToken = default);
    Task<ExportInventory?> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<int> GetNumberOfProductLeftInInventoryThruLotNumberAsync(string lotNumber, ulong productId, CancellationToken cancellationToken = default);
    Task<(List<ExportInventory>, int totalCount)> GetAllExportInventoriesAsync(int page, int pageSize, string? movementType = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllLotNumbersByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
    Task<List<(string LotNumber, int RemainingQuantity)>> GetNumberOfProductsLeftInInventoryThruLotNumbersAsync(List<string> lotNumbers, CancellationToken cancellationToken = default);
    Task<List<(string LotNumber, string SerialNumber)>> GetSerialNumbersWithLotNumbersByProductIdAsync(ulong productId, CancellationToken cancellationToken = default);
    Task<List<ulong>> CreateExportForExportWithTransactionAsync(List<ExportInventory> exportInventories, Dictionary<ulong, int> productQuantities, List<ProductSerial> s, CancellationToken cancellationToken = default);
}