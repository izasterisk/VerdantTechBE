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

            return vendorCertificate;
        }

        public async Task<VendorCertificate> UpdateAsync( ulong id, VendorCertificate vendorCertificate, IEnumerable<MediaLink>? addVendorCertificateFiles, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default)
        {
            var existing = await _context.VendorCertificates
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                throw new KeyNotFoundException($"VendorCertificate ID {id} not found");

            // Update các field cho phép
            existing.CertificationCode = vendorCertificate.CertificationCode;
            existing.CertificationName = vendorCertificate.CertificationName;
            existing.UpdatedAt = DateTime.UtcNow;

            // Remove media theo publicId
            if (removeCertificatePublicIds != null)
            {
                var removeList = await _context.MediaLinks
                    .Where(m =>
                        m.OwnerType == MediaOwnerType.VendorCertificates &&
                        m.OwnerId == id &&
                        m.ImagePublicId != null &&
                        removeCertificatePublicIds.Contains(m.ImagePublicId))
                    .ToListAsync(ct);

                if (removeList.Count > 0)
                {
                    _context.MediaLinks.RemoveRange(removeList);
                }
            }

            // Thêm media mới
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
            return existing;
        }

        public async Task<VendorCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            return await _context.VendorCertificates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<VendorCertificate>> GetAllByVendorIdAsync( ulong vendorId,int page, int pageSize, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            return await _context.VendorCertificates
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task DeleteCertificateAsync(VendorCertificate vendorCertificate, CancellationToken ct = default)
        {
            // Xóa luôn MediaLinks liên quan
            var media = await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    m.OwnerId == vendorCertificate.Id)
                .ToListAsync(ct);

            if (media.Count > 0)
            {
                _context.MediaLinks.RemoveRange(media);
            }

            _context.VendorCertificates.Remove(vendorCertificate);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<VendorCertificate?> ApproveAsync(ulong id, VendorCertificateStatus status, ulong? verifiedByUserId, string? rejectionReason, CancellationToken ct = default)
        {
            var existing = await _context.VendorCertificates
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (existing == null)
                return null;

            existing.Status = status;
            existing.VerifiedBy = verifiedByUserId;
            existing.VerifiedAt = DateTime.UtcNow;
            existing.RejectionReason = rejectionReason;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return existing;
        }
    }
}
