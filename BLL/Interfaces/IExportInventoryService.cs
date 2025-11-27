using BLL.DTO;
using BLL.DTO.ExportInventory;

namespace BLL.Interfaces;

public interface IExportInventoryService
{
    Task<List<ExportInventoryResponseDTO>> CreateExportInventoriesAsync(ulong staffId, List<ExportInventoryCreateDTO> dtos, CancellationToken cancellationToken = default);
    Task<ExportInventoryResponseDTO> GetExportInventoryByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ExportInventoryResponseDTO>> GetAllExportInventoriesAsync(int page, int pageSize, string? movementType = null, CancellationToken cancellationToken = default);
    Task<IdentityNumberDTO> GetIdentityNumbersAsync(ulong productId, CancellationToken cancellationToken = default);
}