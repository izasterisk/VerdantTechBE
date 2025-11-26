using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public sealed class ProductRegistrationRepository : IProductRegistrationRepository
{
    private readonly VerdantTechDbContext _db;

    public ProductRegistrationRepository(VerdantTechDbContext db) => _db = db;

    // ========= READS =========
    public async Task<(IReadOnlyList<ProductRegistration> Items, int Total)> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _db.ProductRegistrations.AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // nạp media (nếu muốn eager load kèm theo)
        await LoadMediaAsync(items.Select(i => i.Id).ToList(), items, ct);

        return (items, total);
    }

    public async Task<ProductRegistration?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _db.ProductRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null) return null;

        await LoadMediaAsync(new List<ulong> { id }, new List<ProductRegistration> { entity }, ct);
        return entity;
    }

    public async Task<(IReadOnlyList<ProductRegistration> Items, int Total)> GetByVendorAsync(
        ulong vendorId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _db.ProductRegistrations
            .AsNoTracking()
            .Where(x => x.VendorId == vendorId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        await LoadMediaAsync(items.Select(i => i.Id).ToList(), items, ct);
        return (items, total);
    }

    // ========= CREATE =========
    public async Task<ProductRegistration> CreateAsync( ProductRegistration registration, IEnumerable<MediaLink>? productImages, IEnumerable<MediaLink>? certificateImages, CancellationToken ct = default)
    {
        // đảm bảo trạng thái mặc định
        if (registration.Status == default)
            registration.Status = ProductRegistrationStatus.Pending;

        // ManualUrls/PublicUrl (manual PDF) đã set vào registration trước khi call repo

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        await _db.ProductRegistrations.AddAsync(registration, ct);
        await _db.SaveChangesAsync(ct); // để có Id

        // insert Product images
        if (productImages != null)
        {
            var startSort = await GetMaxSortAsync(registration.Id, MediaOwnerType.ProductRegistrations, ct);
            var sort = startSort;

            foreach (var m in productImages)
            {
                m.OwnerType = MediaOwnerType.ProductRegistrations;
                m.OwnerId = registration.Id;
                m.SortOrder = ++sort;
                m.CreatedAt = DateTime.UtcNow;
                m.UpdatedAt = DateTime.UtcNow;
                await _db.MediaLinks.AddAsync(m, ct);
            }
        }

        // insert Certificate images
        if (certificateImages != null)
        {
            var startSort = await GetMaxSortAsync(registration.Id, MediaOwnerType.ProductCertificates, ct);
            var sort = startSort;

            foreach (var m in certificateImages)
            {
                m.OwnerType = MediaOwnerType.ProductCertificates;
                m.OwnerId = registration.Id;
                m.SortOrder = ++sort;
                m.CreatedAt = DateTime.UtcNow;
                m.UpdatedAt = DateTime.UtcNow;
                await _db.MediaLinks.AddAsync(m, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // load lại media (nếu cần trả về đầy đủ)
        await LoadMediaAsync(new List<ulong> { registration.Id }, new List<ProductRegistration> { registration }, ct);

        return registration;
    }

    // ========= UPDATE =========
    public async Task<ProductRegistration> UpdateAsync( ProductRegistration registration, IEnumerable<MediaLink>? addProductImages, IEnumerable<MediaLink>? addCertificateImages, IEnumerable<string>? removeImagePublicIds, IEnumerable<string>? removeCertificatePublicIds, CancellationToken ct = default)
    {
        var existing = await _db.ProductRegistrations
            .FirstOrDefaultAsync(x => x.Id == registration.Id, ct);

        if (existing is null)
            throw new KeyNotFoundException("ProductRegistration not found");

        // update các field cơ bản (manual urls/public url, specs, price, dimensions…)
        _db.Entry(existing).CurrentValues.SetValues(registration);
        existing.UpdatedAt = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // remove product images
        if (removeImagePublicIds != null)
        {
            var toRemove = await _db.MediaLinks
                .Where(x => x.OwnerType == MediaOwnerType.ProductRegistrations
                            && x.OwnerId == existing.Id
                            && removeImagePublicIds.Contains(x.ImagePublicId))
                .ToListAsync(ct);
            if (toRemove.Count > 0)
                _db.MediaLinks.RemoveRange(toRemove);
        }

        // remove certificate images
        if (removeCertificatePublicIds != null)
        {
            var toRemove = await _db.MediaLinks
                .Where(x => x.OwnerType == MediaOwnerType.ProductCertificates
                            && x.OwnerId == existing.Id
                            && removeCertificatePublicIds.Contains(x.ImagePublicId))
                .ToListAsync(ct);
            if (toRemove.Count > 0)
                _db.MediaLinks.RemoveRange(toRemove);
        }

        // add product images
        if (addProductImages != null)
        {
            var startSort = await GetMaxSortAsync(existing.Id, MediaOwnerType.ProductRegistrations, ct);
            var sort = startSort;

            foreach (var m in addProductImages)
            {
                m.OwnerType = MediaOwnerType.ProductRegistrations;
                m.OwnerId = existing.Id;
                m.SortOrder = ++sort;
                m.CreatedAt = DateTime.UtcNow;
                m.UpdatedAt = DateTime.UtcNow;
                await _db.MediaLinks.AddAsync(m, ct);
            }
        }

        // add certificate images
        if (addCertificateImages != null)
        {
            var startSort = await GetMaxSortAsync(existing.Id, MediaOwnerType.ProductCertificates, ct);
            var sort = startSort;

            foreach (var m in addCertificateImages)
            {
                m.OwnerType = MediaOwnerType.ProductCertificates;
                m.OwnerId = existing.Id;
                m.SortOrder = ++sort;
                m.CreatedAt = DateTime.UtcNow;
                m.UpdatedAt = DateTime.UtcNow;
                await _db.MediaLinks.AddAsync(m, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // load media cho entity trả về
        await LoadMediaAsync(new List<ulong> { existing.Id }, new List<ProductRegistration> { existing }, ct);
        return existing;
    }

    // ========= STATUS =========
    public async Task<bool> ChangeStatusAsync( ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, DateTime? approvedAt, CancellationToken ct = default)
    {
        var entity = await _db.ProductRegistrations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        entity.Status = status;
        entity.RejectionReason = rejectionReason;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedAt = approvedAt;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ========= DELETE =========
    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _db.ProductRegistrations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // xóa media links liên quan
        var medias = await _db.MediaLinks
            .Where(x => (x.OwnerType == MediaOwnerType.ProductRegistrations || x.OwnerType == MediaOwnerType.ProductCertificates)
                        && x.OwnerId == id)
            .ToListAsync(ct);

        if (medias.Count > 0) _db.MediaLinks.RemoveRange(medias);

        _db.ProductRegistrations.Remove(entity);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    // ========= Helpers =========
    private async Task<int> GetMaxSortAsync(ulong ownerId, MediaOwnerType ownerType, CancellationToken ct)
    {
        var max = await _db.MediaLinks
            .Where(m => m.OwnerId == ownerId && m.OwnerType == ownerType)
            .Select(m => (int?)m.SortOrder)
            .MaxAsync(ct);

        return max ?? 0;
    }

    private async Task LoadMediaAsync( List<ulong> ids, List<ProductRegistration> registrations, CancellationToken ct)
    {
        if (ids.Count == 0) return;

        var byId = registrations.ToDictionary(x => x.Id);

        
        var regMedias = await _db.MediaLinks
            .AsNoTracking()
            .Where(m => m.OwnerType == MediaOwnerType.ProductRegistrations &&
                        ids.Contains(m.OwnerId))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

        foreach (var g in regMedias.GroupBy(m => m.OwnerId))
        {
            if (byId.TryGetValue(g.Key, out var reg))
                reg.ProductImages = g.ToList();
        }

       
        var certs = await _db.ProductCertificates
            .AsNoTracking()
            .Where(c => c.RegistrationId.HasValue &&
                        ids.Contains(c.RegistrationId.Value))
            .Select(c => new
            {
                c.Id,
                c.RegistrationId
            })
            .ToListAsync(ct);

        if (certs.Count == 0) return;

        var certIds = certs.Select(c => c.Id).ToList();

        
        var certFiles = await _db.MediaLinks
            .AsNoTracking()
            .Where(m => m.OwnerType == MediaOwnerType.ProductCertificates &&
                        certIds.Contains(m.OwnerId))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

        
        foreach (var cert in certs)
        {
            if (cert.RegistrationId.HasValue &&
                byId.TryGetValue(cert.RegistrationId.Value, out var reg))
            {
                reg.CertificateFiles ??= new List<MediaLink>();
                reg.CertificateFiles.AddRange(certFiles.Where(f => f.OwnerId == cert.Id));
            }
        }
    }
}
