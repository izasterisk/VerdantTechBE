using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.QualityCheckedByNavigation)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
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
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.QualityCheckedByNavigation)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
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
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.QualityCheckedByNavigation)
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<BatchInventory?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.BatchInventories
                .Include(x => x.Product)
                    .ThenInclude(p => p.Category)
                .Include(x => x.Vendor)
                .Include(x => x.QualityCheckedByNavigation)
                .Include(x => x.ProductSerials)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }


        public async Task<BatchInventory> CreateAsync(BatchInventory entity, CancellationToken ct = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.BatchInventories.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity;
        }

       
        public async Task UpdateAsync(BatchInventory entity, CancellationToken ct = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.BatchInventories.Update(entity);
            await _context.SaveChangesAsync(ct);
        }


        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.BatchInventories
                .Include(x => x.ProductSerials)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return;

            if (entity.ProductSerials.Any())
                _context.ProductSerials.RemoveRange(entity.ProductSerials);

            _context.BatchInventories.Remove(entity);

            await _context.SaveChangesAsync(ct);
        }


        public async Task QualityCheckAsync(
           ulong id,
           QualityCheckStatus status,
           ulong? qualityCheckedByUserId,
           string? notes,
           CancellationToken ct = default)
        {
            using var tran = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var entity = await _context.BatchInventories
                    .Include(x => x.Product)
                    .ThenInclude(p => p.Category)
                    .Include(x => x.ProductSerials)
                    .FirstOrDefaultAsync(x => x.Id == id, ct);

                if (entity == null)
                    return;

                entity.QualityCheckStatus = status;
                entity.QualityCheckedBy = qualityCheckedByUserId;
                entity.QualityCheckedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(notes))
                {
                    entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                        ? notes
                        : $"{entity.Notes}\n{notes}";
                }

                await _context.SaveChangesAsync(ct);

                bool serialRequired = entity.Product.Category?.SerialRequired ?? false;

                if (serialRequired &&
                    status == QualityCheckStatus.Passed &&
                    !entity.ProductSerials.Any())
                {
                    var serials = new List<ProductSerial>();
                    var now = DateTime.UtcNow;

                    for (int i = 0; i < entity.Quantity; i++)
                    {
                        serials.Add(new ProductSerial
                        {
                            BatchInventoryId = entity.Id,
                            ProductId = entity.ProductId,
                            SerialNumber = $"{entity.Sku}-{entity.BatchNumber}-{i:D3}",
                            Status = ProductSerialStatus.Stock,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }

                    await _context.ProductSerials.AddRangeAsync(serials, ct);
                    await _context.SaveChangesAsync(ct);
                }

                await tran.CommitAsync(ct);
            }
            catch
            {
                await tran.RollbackAsync(ct);
                throw;
            }
        }
    }
}
