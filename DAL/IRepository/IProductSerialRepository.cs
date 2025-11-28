using DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IProductSerialRepository
    {
        Task<ProductSerial?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<IEnumerable<ProductSerial>> GetAllByProductIdAsync(ulong productId, CancellationToken ct = default);
        Task<IEnumerable<ProductSerial>> GetAllByBatchIdAsync(ulong batchId, CancellationToken ct = default);
        Task CreateAsync(ProductSerial serial, CancellationToken ct = default);
        Task UpdateAsync(ProductSerial entity, CancellationToken ct = default);
        Task DeleteRangeAsync(IEnumerable<ProductSerial> list, CancellationToken ct = default);
    }
}
