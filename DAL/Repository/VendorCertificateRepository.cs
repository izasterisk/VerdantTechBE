using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public sealed class VendorCertificateRepository : IVendorCertificateRepository
    {
        private readonly VerdantTechDbContext _db;
        public VendorCertificateRepository(VerdantTechDbContext db) => _db = db;


        public async Task<VendorCertificate> CreateAsync(
        ulong id, // kept to match your interface
        VendorCertificate vendorcertificate,
        IEnumerable<MediaLink>? addVendorCertificateFiles,
        CancellationToken ct = default)
        {
            if (vendorcertificate is null) throw new ArgumentNullException(nameof(vendorcertificate));


            _db.Set<VendorCertificate>().Add(vendorcertificate);


            if (addVendorCertificateFiles is not null)
            {
                foreach (var m in addVendorCertificateFiles)
                {
                    if (m is null) continue;
                    // In your schema, MediaLink.Id is the owner id (certificate id)
                    m.Id = id == 0 ? vendorcertificate.Id : id;
                    m.OwnerType = MediaOwnerType.VendorCertificates;
                    m.CreatedAt = m.CreatedAt == default ? DateTime.UtcNow : m.CreatedAt;
                    _db.Set<MediaLink>().Add(m);
                }
            }


            await _db.SaveChangesAsync(ct);
            return vendorcertificate;
        }


        public async Task<VendorCertificate> UpdateAsync(
        ulong id,
        VendorCertificate vendorcertificate,
        IEnumerable<MediaLink>? addVendorCertificateFiles,
        IEnumerable<string>? removeCertificatePublicIds,
        CancellationToken ct = default)
        {
            if (vendorcertificate is null) throw new ArgumentNullException(nameof(vendorcertificate));


            _db.Set<VendorCertificate>().Update(vendorcertificate);


            if (addVendorCertificateFiles is not null)
            {
                foreach (var m in addVendorCertificateFiles)
                {
                    if (m is null) continue;
                    m.Id = id == 0 ? vendorcertificate.Id : id;
                    m.OwnerType = MediaOwnerType.VendorCertificates;
                    m.CreatedAt = m.CreatedAt == default ? DateTime.UtcNow : m.CreatedAt;
                    _db.Set<MediaLink>().Add(m);
                }
            }


            if (removeCertificatePublicIds is not null)
            {
                var toRemove = await _db.Set<MediaLink>()
                .Where(x => x.OwnerType == MediaOwnerType.VendorCertificates
                && x.Id == vendorcertificate.Id
                && removeCertificatePublicIds.Contains(x.ImagePublicId))
                .ToListAsync(ct);
                if (toRemove.Count > 0)
                {
                    _db.Set<MediaLink>().RemoveRange(toRemove);
                }
            }


            await _db.SaveChangesAsync(ct);
            return vendorcertificate;
        }


        public Task<VendorCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default)
        => _db.Set<VendorCertificate>()
        .AsNoTracking()
        .FirstOrDefaultAsync(v => v.Id == id, ct);


        public Task<List<VendorCertificate>> GetAllByVendorIdAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        {
            
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            var skip = (page - 1) * pageSize;


            return _db.Set<VendorCertificate>()
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
        }


        public async Task DeleteCertificateAsync(VendorCertificate vendorcertificatey, CancellationToken ct = default)
        {
            if (vendorcertificatey is null) throw new ArgumentNullException(nameof(vendorcertificatey));


            // Delete related medias first
            var medias = await _db.Set<MediaLink>()
            .Where(m => m.OwnerType == MediaOwnerType.VendorCertificates
            && m.Id == vendorcertificatey.Id)
            .ToListAsync(ct);
            if (medias.Count > 0)
            {
                _db.Set<MediaLink>().RemoveRange(medias);
            }


            _db.Set<VendorCertificate>().Remove(vendorcertificatey);
            await _db.SaveChangesAsync(ct);
        }
    }
}