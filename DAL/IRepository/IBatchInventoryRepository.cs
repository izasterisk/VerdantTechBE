using DAL.Data.Models;
using DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IBatchInventoryRepository
    {
        Task<IEnumerable<BatchInventory>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventory>> GetByProductIdAsync(ulong productId, int page, int pageSize, CancellationToken ct = default);
        Task<IEnumerable<BatchInventory>> GetByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<BatchInventory?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<BatchInventory> CreateAsync(BatchInventory entity, CancellationToken ct = default);
        Task UpdateAsync(BatchInventory entity, CancellationToken ct = default);
        Task DeleteAsync(ulong id, CancellationToken ct = default);
        Task QualityCheckAsync(ulong id, QualityCheckStatus status, ulong? qualityCheckedByUserId, string? notes, CancellationToken ct = default);
    }
}
