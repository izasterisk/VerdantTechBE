using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL.Repository
{
    public class CropRepository : ICropRepository
    {
        private readonly  VerdantTechDbContext _context;

        public CropRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Crop>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _context.Crops
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<Crop?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.Crops
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task AddAsync(Crop crop, CancellationToken ct = default)
        {
            await _context.Crops.AddAsync(crop, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Crop crop, CancellationToken ct = default)
        {
            _context.Crops.Update(crop);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> SoftDeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.Crops.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.Crops.Update(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> HardDeleteAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _context.Crops.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity == null) return false;

            _context.Crops.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<Crop>> GetByFarmIdAsync(ulong farmProfileId, CancellationToken ct = default)
        {
            return await _context.Crops
                .AsNoTracking()
                .Where(x => x.FarmProfileId == farmProfileId && x.IsActive)
                .ToListAsync(ct);
        }

    }
}
