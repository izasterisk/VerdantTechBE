using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class ProductSerialRepository : IProductSerialRepository
    {
        private readonly VerdantTechDbContext _context;

        public ProductSerialRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        public async Task<ProductSerial?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.ProductSerials
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IEnumerable<ProductSerial>> GetAllByProductIdAsync(ulong productId, CancellationToken ct = default)
        {
            return await _context.ProductSerials
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<ProductSerial>> GetAllByBatchIdAsync(ulong batchId, CancellationToken ct = default)
        {
            return await _context.ProductSerials
                .Where(x => x.BatchInventoryId == batchId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task CreateAsync(ProductSerial serial, CancellationToken ct = default)
        {
            await _context.ProductSerials.AddAsync(serial, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ProductSerial entity, CancellationToken ct = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProductSerials.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteRangeAsync(IEnumerable<ProductSerial> list, CancellationToken ct = default)
        {
            _context.ProductSerials.RemoveRange(list);
            await _context.SaveChangesAsync(ct);
        }
    }
}
