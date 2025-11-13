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
        Task<IEnumerable<BatchInventoryResponeDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventoryResponeDTO>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventoryResponeDTO>> GetByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<BatchInventoryResponeDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<BatchInventoryResponeDTO> CreateAsync(BatchInventoryCreateDTO dto, CancellationToken ct = default);
        Task<BatchInventoryResponeDTO> UpdateAsync(BatchInventoryUpdateDTO dto, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task QualityCheckAsync(ulong id, BatchInventoryQualityCheckDTO dto, CancellationToken ct = default);
    }
}
