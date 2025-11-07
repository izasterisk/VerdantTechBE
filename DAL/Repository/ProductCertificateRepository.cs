using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;            
using System.Threading.Tasks;

namespace DAL.Repository
{
   public class ProductCertificateRepository : IProductCertificateRepository
    {
        private readonly VerdantTechDbContext _db;
        public ProductCertificateRepository(VerdantTechDbContext db) => _db = db;

        // ===== READS =====
        public async Task<(IReadOnlyList<ProductCertificate> Items, int Total)> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1; if (pageSize < 1) pageSize = 10;
            var q = _db.ProductCertificates.AsNoTracking().OrderByDescending(x => x.UploadedAt);
            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }

        public Task<ProductCertificate?> GetByIdAsync(ulong id, CancellationToken ct = default)
            => _db.ProductCertificates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(IReadOnlyList<ProductCertificate> Items, int Total)> GetByProductAsync(ulong productId, int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1; if (pageSize < 1) pageSize = 10;
            var q = _db.ProductCertificates.AsNoTracking()
                                           .Where(x => x.ProductId == productId)
                                           .OrderByDescending(x => x.UploadedAt);
            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return (items, total);
        }

        // ===== CREATE / UPDATE (transaction + MediaLink) =====
        public async Task<ProductCertificate> CreateAsync(ProductCertificate certificate, IEnumerable<MediaLink>? addCertificateFiles, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;
                certificate.CreatedAt = now;
                certificate.UpdatedAt = now;
                if (certificate.UploadedAt == default) certificate.UploadedAt = now;

                await _db.ProductCertificates.AddAsync(certificate, ct);
                await _db.SaveChangesAsync(ct);

                if (addCertificateFiles != null)
                {
                    var adds = addCertificateFiles
                        .Where(f => f != null)
                        .Select((f, idx) => new MediaLink
                        {
                            OwnerType = MediaOwnerType.ProductCertificates,
                            OwnerId = certificate.Id,
                            ImagePublicId = f.ImagePublicId,
                            ImageUrl = f.ImageUrl,
                            Purpose = f.Purpose == 0 ? MediaPurpose.CertificatePdf : f.Purpose,
                            SortOrder = f.SortOrder == 0 ? idx : f.SortOrder,
                            CreatedAt = now,
                            UpdatedAt = now
                        }).ToList();

                    if (adds.Count > 0)
                    {
                        await _db.MediaLinks.AddRangeAsync(adds, ct);
                        await _db.SaveChangesAsync(ct);
                    }
                }

                await tx.CommitAsync(ct);
                return certificate;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<ProductCertificate> UpdateAsync(ProductCertificate certificate, IEnumerable<MediaLink>? addCertificateFiles, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var entity = await _db.ProductCertificates.FirstOrDefaultAsync(x => x.Id == certificate.Id, ct)
                             ?? throw new KeyNotFoundException("Không tìm thấy chứng chỉ.");

                if (certificate.ProductId != default) entity.ProductId = certificate.ProductId;
                if (!string.IsNullOrWhiteSpace(certificate.CertificationCode)) entity.CertificationCode = certificate.CertificationCode;
                if (!string.IsNullOrWhiteSpace(certificate.CertificationName)) entity.CertificationName = certificate.CertificationName;
                if (certificate.Status != 0) entity.Status = certificate.Status; // enum 0 = Pending
                if (certificate.RejectionReason != null) entity.RejectionReason = certificate.RejectionReason;
                entity.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(ct);

                // remove files
                if (removeCertificatePublicIds != null && removeCertificatePublicIds.Any())
                {
                    var toRemove = await _db.MediaLinks
                        .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates
                                 && m.OwnerId == entity.Id
                                 && m.Purpose == MediaPurpose.CertificatePdf
                                 && removeCertificatePublicIds.Contains(m.ImagePublicId))
                        .ToListAsync(ct);

                    if (toRemove.Count > 0)
                    {
                        _db.MediaLinks.RemoveRange(toRemove);
                        await _db.SaveChangesAsync(ct);
                    }
                }

                // add files
                if (addCertificateFiles != null)
                {
                    var now = DateTime.UtcNow;
                    var adds = addCertificateFiles.Where(f => f != null)
                        .Select((f, idx) => new MediaLink
                        {
                            OwnerType = MediaOwnerType.ProductCertificates,
                            OwnerId = entity.Id,
                            ImagePublicId = f.ImagePublicId,
                            ImageUrl = f.ImageUrl,
                            Purpose = f.Purpose == 0 ? MediaPurpose.CertificatePdf : f.Purpose,
                            SortOrder = f.SortOrder == 0 ? idx : f.SortOrder,
                            CreatedAt = now,
                            UpdatedAt = now
                        }).ToList();

                    if (adds.Count > 0)
                    {
                        await _db.MediaLinks.AddRangeAsync(adds, ct);
                        await _db.SaveChangesAsync(ct);
                    }
                }

                await tx.CommitAsync(ct);
                return entity;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        // ===== STATUS / DELETE =====
        public async Task<bool> ChangeStatusAsync(ulong id, ProductCertificateStatus status, string? rejectionReason, ulong? verifiedBy, DateTime? verifiedAt, CancellationToken ct = default)
        {
            var e = await _db.ProductCertificates.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (e == null) return false;

            e.Status = status;
            e.RejectionReason = status == ProductCertificateStatus.Rejected ? rejectionReason : null;
            e.VerifiedBy = verifiedBy;
            e.VerifiedAt = status is ProductCertificateStatus.Verified or ProductCertificateStatus.Rejected
                ? (verifiedAt ?? DateTime.UtcNow)
                : null;
            e.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var e = await _db.ProductCertificates.FirstOrDefaultAsync(x => x.Id == id, ct);
                if (e == null) return false;

                var links = await _db.MediaLinks
                    .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates
                             && m.OwnerId == id
                             && m.Purpose == MediaPurpose.CertificatePdf)
                    .ToListAsync(ct);

                if (links.Count > 0) _db.MediaLinks.RemoveRange(links);
                _db.ProductCertificates.Remove(e);
                await _db.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
