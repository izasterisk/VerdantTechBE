using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class BatchInventoryRepository : IBatchInventoryRepository
    {
        private readonly VerdantTechDbContext _context;

        public BatchInventoryRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BatchInventory>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            return await _context.Set<BatchInventory>()
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<BatchInventory>> GetByProductIdAsync(
            ulong productId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            return await _context.Set<BatchInventory>()
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<BatchInventory>> GetByVendorIdAsync(
            ulong vendorId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            return await _context.Set<BatchInventory>()
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<BatchInventory?> GetByIdAsync(
            ulong id,
            CancellationToken ct = default)
        {
            return await _context.Set<BatchInventory>()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<BatchInventory> CreateAsync(
            BatchInventory entity,
            CancellationToken ct = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.Set<BatchInventory>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(
            BatchInventory entity,
            CancellationToken ct = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Set<BatchInventory>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(
            ulong id,
            CancellationToken ct = default)
        {
            var entity = await _context.Set<BatchInventory>()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return;

            _context.Set<BatchInventory>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task QualityCheckAsync(
            ulong id,
            QualityCheckStatus status,
            ulong? qualityCheckedByUserId,
            string? notes,
            CancellationToken ct = default)
        {
            var entity = await _context.Set<BatchInventory>()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return;

            entity.QualityCheckStatus = status;
            entity.QualityCheckedBy = qualityCheckedByUserId;
            entity.QualityCheckedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(notes))
            {
                entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                    ? notes
                    : $"{entity.Notes}\n{notes}";
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}
