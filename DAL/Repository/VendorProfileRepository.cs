using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DAL.IRepository;
using DAL.Data;
using DAL.Data.Models;


namespace DAL.Repository
{
    public sealed class VendorProfileRepository : IVendorProfileRepository
    {
        private readonly VerdantTechDbContext _db;
        public VendorProfileRepository(VerdantTechDbContext db) => _db = db;


        public async Task<VendorProfile> CreateAsync(
        VendorProfile vendorProfile,
        IEnumerable<MediaLink>? addVendorCertificateFiles,
        CancellationToken ct = default)
        {
            if (vendorProfile is null) throw new ArgumentNullException(nameof(vendorProfile));


            _db.Set<VendorProfile>().Add(vendorProfile);


            // Optional: attach MediaLink entries (owner is VendorProfile.Id in your current usage only if desired).
            // Your current convention uses MediaLink for certificates; keep this block only when requested.
            if (addVendorCertificateFiles is not null)
            {
                foreach (var m in addVendorCertificateFiles)
                {
                    if (m is null) continue;
                    m.Id = vendorProfile.Id; // EF will populate after SaveChanges
                    m.OwnerType = MediaOwnerType.VendorCertificates;
                    m.CreatedAt = m.CreatedAt == default ? DateTime.UtcNow : m.CreatedAt;
                    _db.Set<MediaLink>().Add(m);
                }
            }


            await _db.SaveChangesAsync(ct);
            return vendorProfile;
        }


        public Task<VendorProfile?> GetByIdAsync(ulong id, CancellationToken ct = default)
        => _db.Set<VendorProfile>()
        .AsNoTracking()
        .FirstOrDefaultAsync(v => v.Id == id, ct);


        public Task<List<VendorProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            var skip = (page - 1) * pageSize;


            return _db.Set<VendorProfile>()
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
        }


        public async Task UpdateAsync(VendorProfile vendorProfile, CancellationToken ct = default)
        {
            if (vendorProfile is null) throw new ArgumentNullException(nameof(vendorProfile));
            _db.Set<VendorProfile>().Update(vendorProfile);
            await _db.SaveChangesAsync(ct);
        }


        public async Task DeleteAsync(VendorProfile vendorProfile, CancellationToken ct = default)
        {
            if (vendorProfile is null) throw new ArgumentNullException(nameof(vendorProfile));
            _db.Set<VendorProfile>().Remove(vendorProfile);
            await _db.SaveChangesAsync(ct);
        }
    }
}