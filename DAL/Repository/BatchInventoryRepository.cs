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
                .Include(x => x.Vendor)
                .Include(x => x.QualityCheckedByNavigation)
                .Include(x => x.ProductSerials)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        //public async Task<BatchInventory> CreateAsync(
        //    BatchInventory entity,
        //    CancellationToken ct = default)
        //{
        //    entity.CreatedAt = DateTime.UtcNow;
        //    entity.UpdatedAt = DateTime.UtcNow;

        //    await _context.Set<BatchInventory>().AddAsync(entity, ct);
        //    await _context.SaveChangesAsync(ct);
        //    return entity;
        //}


        public async Task<BatchInventory> CreateAsync(BatchInventory entity, CancellationToken ct = default)
        {
            using var tran = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == entity.ProductId, ct);

                if (product == null)
                    throw new InvalidOperationException($"Product {entity.ProductId} not found.");

                bool serialRequired = product.Category?.SerialRequired ?? false;

                // VALIDATE
                if (serialRequired)
                {
                    if (entity.Quantity <= 0)
                        throw new InvalidOperationException("Quantity must be > 0 for serial-required products.");

                    if (string.IsNullOrEmpty(entity.LotNumber))
                        throw new InvalidOperationException("LotNumber is required for serial-managed products.");
                }

                // 1) Insert batch
                await _context.BatchInventories.AddAsync(entity, ct);
                await _context.SaveChangesAsync(ct);

                // 2) If category requires serial → generate
                if (serialRequired)
                {
                    var serials = new List<ProductSerial>();
                    var now = DateTime.UtcNow;

                    for (int i = 0; i < entity.Quantity; i++)
                    {
                        serials.Add(new ProductSerial
                        {
                            BatchInventoryId = entity.Id,
                            ProductId = entity.ProductId,
                            SerialNumber = GenerateSerialNumber(),
                            Status = ProductSerialStatus.Stock,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }

                    await _context.ProductSerials.AddRangeAsync(serials, ct);
                    await _context.SaveChangesAsync(ct);
                }

                await tran.CommitAsync(ct);

                return entity;
            }
            catch
            {
                await tran.RollbackAsync(ct);
                throw;
            }
        }

        private string GenerateSerialNumber()
        {
            return Guid.NewGuid().ToString("N");
        }


        public async Task<BatchInventory> CreateAsync(
            BatchInventory entity,
            CancellationToken ct = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == entity.ProductId, ct);

            if (product == null)
                throw new InvalidOperationException($"Product {entity.ProductId} not found.");

            bool serialRequired = product.Category?.SerialRequired ?? false;

            if (serialRequired)
            {
                if (entity.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be > 0 for serial-required products.");

                if (string.IsNullOrWhiteSpace(entity.LotNumber))
                    throw new InvalidOperationException("LotNumber is required for serial-managed products.");
            }

            await _context.BatchInventories.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            if (serialRequired)
            {
                for (int i = 1; i <= entity.Quantity; i++)
                {
                    var serial = new ProductSerial
                    {
                        BatchInventoryId = entity.Id,
                        ProductId = entity.ProductId,
                        SerialNumber = GenerateSerialNumber(entity, i),
                        Status = ProductSerialStatus.Stock,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.ProductSerials.AddAsync(serial, ct);
                }

                await _context.SaveChangesAsync(ct);
            }

            return entity;
        }

        private string GenerateSerialNumber(BatchInventory batch, int index)
        {
            return $"{batch.Sku}-{batch.BatchNumber}-{index:D3}";
        }



        public async Task UpdateAsync(
            BatchInventory entity,
            CancellationToken ct = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Set<BatchInventory>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.BatchInventories
                .Include(x => x.ProductSerials)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null) return;

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
