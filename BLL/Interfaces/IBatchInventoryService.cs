using BLL.DTO.BatchInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IBatchInventoryService
    {
        Task<IEnumerable<BatchInventoryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventoryDto>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventoryDto>> GetByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<BatchInventoryDto?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<BatchInventoryDto> CreateAsync(BatchInventoryCreateDto dto, CancellationToken ct = default);
        Task<BatchInventoryDto> UpdateAsync(BatchInventoryUpdateDto dto, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task QualityCheckAsync(ulong id, BatchInventoryQualityCheckDto dto, CancellationToken ct = default);
    }
}
