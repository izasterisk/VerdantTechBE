using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface ICropRepository
    {
        Task<IEnumerable<Crop>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<Crop?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<IEnumerable<Crop>> GetByFarmIdAsync(ulong farmProfileId, CancellationToken ct = default);
        Task AddAsync(Crop crop, CancellationToken ct = default);
        Task UpdateAsync(Crop crop, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default);
        Task<bool> HardDeleteAsync(ulong id, CancellationToken ct = default);
    }
}
