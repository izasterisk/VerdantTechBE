using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class VendorProfileRepository : IVendorProfileRepository
    {
        private readonly VerdantTechDbContext _context;

        public VendorProfileRepository(VerdantTechDbContext context)
        {
            _context = context;
        }

        private async Task<List<MediaLink>> LoadFilesAsync(ulong ownerId, CancellationToken ct)
        {
            return await _context.MediaLinks
                .Where(m =>
                    m.OwnerType == MediaOwnerType.VendorCertificates &&
                    m.OwnerId == ownerId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);
        }

        public async Task<VendorProfile> CreateAsync(
            VendorProfile vendorProfile,
            IEnumerable<MediaLink>? addVendorCertificateFiles,
            CancellationToken ct = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // Các field khác (CompanyName, CompanyAddress, Province, ...) 
                // đã được set sẵn ở service trước khi truyền xuống repo.
                vendorProfile.CreatedAt = DateTime.UtcNow;
                vendorProfile.UpdatedAt = DateTime.UtcNow;

                _context.VendorProfiles.Add(vendorProfile);
                await _context.SaveChangesAsync(ct);

                // Insert MediaLink nếu có
                if (addVendorCertificateFiles != null)
                {
                    foreach (var media in addVendorCertificateFiles)
                    {
                        media.OwnerType = MediaOwnerType.VendorCertificates;
                        media.OwnerId = vendorProfile.Id;
                        media.CreatedAt = DateTime.UtcNow;
                        media.UpdatedAt = DateTime.UtcNow;

                        _context.MediaLinks.Add(media);
                    }

                    await _context.SaveChangesAsync(ct);
                }

                vendorProfile.MediaLinks = await LoadFilesAsync(vendorProfile.Id, ct);
                if (vendorProfile.MediaLinks != null)
                {
                    vendorProfile.MediaLinks = vendorProfile.MediaLinks
                        .OrderBy(m => m.SortOrder)
                        .ToList();
                }

                await transaction.CommitAsync(ct);
                return vendorProfile;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<VendorProfile?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var vendorProfile = await _context.VendorProfiles
                .AsNoTracking()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id, ct);

            if (vendorProfile == null)
                return null;

            vendorProfile.MediaLinks = await LoadFilesAsync(vendorProfile.Id, ct);
            if (vendorProfile.MediaLinks != null)
            {
                vendorProfile.MediaLinks = vendorProfile.MediaLinks
                    .OrderBy(m => m.SortOrder)
                    .ToList();
            }

            return vendorProfile;
        }


        public async Task<VendorProfile?> GetByUserIdAsync(ulong vendorId, CancellationToken ct = default)
        {
            var vendorProfile = await _context.VendorProfiles
                .AsNoTracking()
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.UserId == vendorId, ct);

            if (vendorProfile == null)
                return null;

            vendorProfile.MediaLinks = await LoadFilesAsync(vendorProfile.Id, ct);
            if (vendorProfile.MediaLinks != null)
            {
                vendorProfile.MediaLinks = vendorProfile.MediaLinks
                    .OrderBy(m => m.SortOrder)
                    .ToList();
            }

            return vendorProfile;
        }

        public async Task<List<VendorProfile>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.VendorProfiles
                .AsNoTracking()
                .Include(v => v.User)
                .Where(v => v.User.Status == UserStatus.Active);

            var vendorProfiles = await query
                .OrderBy(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var ids = vendorProfiles.Select(v => v.Id).ToList();

            if (ids.Any())
            {
                var mediaLinks = await _context.MediaLinks
                    .Where(m =>
                        m.OwnerType == MediaOwnerType.VendorCertificates &&
                        ids.Contains(m.OwnerId))
                    .OrderBy(m => m.SortOrder)
                    .ToListAsync(ct);

                foreach (var vp in vendorProfiles)
                {
                    vp.MediaLinks = mediaLinks
                        .Where(m => m.OwnerId == vp.Id)
                        .ToList();
                }
            }

            return vendorProfiles;
        }

        public async Task UpdateAsync(VendorProfile vendorProfile, CancellationToken ct = default)
        {
            var existing = await _context.VendorProfiles
                .FirstOrDefaultAsync(v => v.Id == vendorProfile.Id, ct);

            if (existing == null)
                throw new KeyNotFoundException("VendorProfile không tồn tại");

            // Thông tin cơ bản
            existing.CompanyName = vendorProfile.CompanyName;
            existing.Slug = vendorProfile.Slug;
            existing.BusinessRegistrationNumber = vendorProfile.BusinessRegistrationNumber;

            //// Thông tin địa chỉ (CHỈ set được nếu trong entity VendorProfile có các field này)
            //existing.CompanyAddress = vendorProfile.CompanyAddress;
            //existing.Province = vendorProfile.Province;
            //existing.District = vendorProfile.District;
            //existing.Commune = vendorProfile.Commune;

            // Thông tin verify (phục vụ duyệt / từ chối)
            existing.VerifiedAt = vendorProfile.VerifiedAt;
            existing.VerifiedBy = vendorProfile.VerifiedBy;

            existing.UpdatedAt = DateTime.UtcNow;

            _context.VendorProfiles.Update(existing);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(VendorProfile vendorProfile, CancellationToken ct = default)
        {
            _context.VendorProfiles.Remove(vendorProfile);
            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAccountAsync(ulong userId, CancellationToken ct = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null)
                throw new KeyNotFoundException("User không tồn tại");

            user.Status = UserStatus.Inactive;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}
