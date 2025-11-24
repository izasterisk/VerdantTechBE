using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;


namespace DAL.Repository
{
    public class VendorCertificateRepository : IVendorCertificateRepository
    {
        private readonly VerdantTechDbContext _context;

        public VendorCertificateRepository(VerdantTechDbContext context)
        {
            _context = context;
        }


        private async Task<List<MediaLink>> LoadFilesAsync(ulong certId, CancellationToken ct)
        {
            return await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    m.OwnerId == certId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);
        }

        public async Task<VendorCertificate> CreateAsync( ulong vendorId, VendorCertificate vendorCertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, CancellationToken ct = default)
        {
            vendorCertificate.VendorId = vendorId;
            vendorCertificate.Status = VendorCertificateStatus.Pending;
            vendorCertificate.UploadedAt = DateTime.UtcNow;
            vendorCertificate.CreatedAt = DateTime.UtcNow;
            vendorCertificate.UpdatedAt = DateTime.UtcNow;

            _context.VendorCertificates.Add(vendorCertificate);
            await _context.SaveChangesAsync(ct);

            if (addVendorCertificateFiles != null)
            {
                foreach (var media in addVendorCertificateFiles)
                {
                    media.OwnerType = MediaOwnerType.VendorCertificates; 
                    media.OwnerId = vendorCertificate.Id;
                    media.CreatedAt = DateTime.UtcNow;
                    media.UpdatedAt = DateTime.UtcNow;
                    _context.MediaLinks.Add(media);
                }

                await _context.SaveChangesAsync(ct);
            }
            vendorCertificate.MediaLinks = await LoadFilesAsync(vendorCertificate.Id, ct);
            if (vendorCertificate.MediaLinks != null)
            {
                vendorCertificate.MediaLinks = vendorCertificate.MediaLinks
                    .OrderBy(m => m.SortOrder)
                    .ToList();
            }

            return vendorCertificate;
        }


        public async Task<VendorCertificate> UpdateAsync( ulong id, VendorCertificate vendorCertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default)
        {
            var existing = await _context.VendorCertificates
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                throw new KeyNotFoundException($"VendorCertificate ID {id} not found");


            existing.VendorId = vendorCertificate.VendorId;
            existing.CertificationCode = vendorCertificate.CertificationCode;
            existing.CertificationName = vendorCertificate.CertificationName;
            existing.UpdatedAt = DateTime.UtcNow;

            // Remove media 
            if (removeCertificatePublicIds != null)
            {
                var removeList = await _context.MediaLinks
                    .Where(m =>
                        m.OwnerType == MediaOwnerType.VendorCertificates &&
                        m.OwnerId == id &&
                        
                        removeCertificatePublicIds.Contains(m.ImagePublicId))
                    .ToListAsync(ct);

                if (removeList.Any())
                {
                    _context.MediaLinks.RemoveRange(removeList);
                }
            }

            // Add new files
            if (addVendorCertificateFiles != null)
            {
                foreach (var media in addVendorCertificateFiles)
                {
                    media.OwnerType = MediaOwnerType.VendorCertificates;
                    media.OwnerId = existing.Id;
                    media.CreatedAt = DateTime.UtcNow;
                    media.UpdatedAt = DateTime.UtcNow;
                    _context.MediaLinks.Add(media);
                }
            }

            await _context.SaveChangesAsync(ct);

            existing.MediaLinks = await LoadFilesAsync(existing.Id, ct);
            if (existing.MediaLinks != null)
                {
                existing.MediaLinks = existing.MediaLinks
                    .OrderBy(m => m.SortOrder)
                    .ToList();
            }
            return existing;
        }

        public async Task<VendorCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var cert = await _context.VendorCertificates
                 .AsNoTracking()
                 .FirstOrDefaultAsync(v => v.Id == id, ct);

            if (cert == null)
                return null;

            cert.MediaLinks = await LoadFilesAsync(cert.Id, ct);
            if (cert.MediaLinks != null)
                {
                cert.MediaLinks = cert.MediaLinks
                    .OrderBy(m => m.SortOrder)
                    .ToList();
            }

            return cert;
        }

        public async Task<List<VendorCertificate>> GetAllByVendorIdAsync( ulong vendorId,int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.VendorCertificates
                .AsNoTracking()
                .Where(v => v.VendorId == vendorId)
                .OrderByDescending(v => v.UploadedAt);

            var certs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var ids = certs.Select(c => c.Id).ToList();

            var files = await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    ids.Contains(m.OwnerId))
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            foreach (var cert in certs)
            {
                cert.MediaLinks = files.Where(f => f.OwnerId == cert.Id).ToList();
                if (cert.MediaLinks != null)
                {
                    cert.MediaLinks = cert.MediaLinks
                        .OrderBy(m => m.SortOrder)
                        .ToList();
                }
            }

            return certs;
        }

        public async Task DeleteCertificateAsync(VendorCertificate vendorCertificate, CancellationToken ct = default)
        {
            // Xóa luôn MediaLinks liên quan
            var media = await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    m.OwnerId == vendorCertificate.Id)
                .ToListAsync(ct);

            if (media.Any())
            {
                _context.MediaLinks.RemoveRange(media);
            }

            _context.VendorCertificates.Remove(vendorCertificate);

            await _context.SaveChangesAsync(ct);
        }

        public async Task<VendorCertificate?> ApproveAsync(ulong id, VendorCertificateStatus status, ulong? verifiedByUserId, string? rejectionReason, CancellationToken ct = default)
        {
            var existing = await _context.VendorCertificates.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                return null;

            if (verifiedByUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == verifiedByUserId.Value, ct);

                if (!userExists)
                    throw new KeyNotFoundException($"User {verifiedByUserId} not found");
            }

            existing.Status = status;
            existing.VerifiedBy = verifiedByUserId;
            existing.VerifiedAt = verifiedByUserId.HasValue ? DateTime.UtcNow : null;
            existing.RejectionReason = rejectionReason;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            existing.MediaLinks = await _context.MediaLinks
                .Where(m => m.OwnerType == MediaOwnerType.VendorCertificates &&
                            m.OwnerId == existing.Id)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            return existing;
        }

        public async Task DeleteAllByVendorIdAsync(ulong vendorId, CancellationToken ct = default)
        {
            var certs = await _context.VendorCertificates
                .Where(c => c.VendorId == vendorId)
                .ToListAsync(ct);

            if (!certs.Any())
                return;

            var certIds = certs.Select(c => c.Id).ToList();
            var medias = await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    certIds.Contains(m.OwnerId))
                .ToListAsync(ct);

            if (medias.Any())
            {
                _context.MediaLinks.RemoveRange(medias);
            }

            _context.VendorCertificates.RemoveRange(certs);

            await _context.SaveChangesAsync(ct);
        }

    }
}
