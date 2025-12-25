using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository
{
    public class VendorProfileRepository : IVendorProfileRepository
    {
        private readonly VerdantTechDbContext _context;
        private readonly IRepository<VendorProfile> _vendorProfileRepository; 
        private readonly IRepository<Transaction> _transactionRepository;
        
        public VendorProfileRepository(VerdantTechDbContext context, IRepository<VendorProfile> vendorProfileRepository,
            IRepository<Transaction> transactionRepository)
        {
            _context = context;
            _vendorProfileRepository = vendorProfileRepository;
            _transactionRepository = transactionRepository;
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
                .Include(v => v.User);
                //.Where(v => v.User.Status == UserStatus.Active);

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

            //existing.CompanyAddress = vendorProfile.CompanyAddress;
            //existing.Province = vendorProfile.Province;
            //existing.District = vendorProfile.District;
            //existing.Commune = vendorProfile.Commune;

            // Thông tin verify (phục vụ duyệt / từ chối)
            existing.Notes = vendorProfile.Notes;
            existing.VerifiedAt = vendorProfile.VerifiedAt;
            existing.VerifiedBy = vendorProfile.VerifiedBy;

            existing.UpdatedAt = DateTime.UtcNow;

            _context.VendorProfiles.Update(existing);
            await _context.SaveChangesAsync(ct);
        }

        //public async Task DeleteAsync(VendorProfile vendorProfile, CancellationToken ct = default)
        //{
        //    _context.VendorProfiles.Remove(vendorProfile);
        //    await _context.SaveChangesAsync(ct);
        //}

        public async Task HardDeleteVendorAsync(ulong vendorProfileId, CancellationToken ct)
        {
            var vp = await _context.VendorProfiles.FirstOrDefaultAsync(v => v.Id == vendorProfileId, ct);
            if (vp == null)
                return;

            ulong userId = vp.UserId;

            var certs = await _context.VendorCertificates
                .Where(c => c.VendorId == userId)
                .ToListAsync(ct);

            if (certs.Any())
            {
                var certIds = certs.Select(c => c.Id).ToList();

                var medias = await _context.MediaLinks
                    .Where(m => m.OwnerType == MediaOwnerType.VendorCertificates &&
                                certIds.Contains(m.OwnerId))
                    .ToListAsync(ct);

                _context.MediaLinks.RemoveRange(medias);
                _context.VendorCertificates.RemoveRange(certs);
            }

            var userAddresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync(ct);

            if (userAddresses.Any())
            {
                var addressIds = userAddresses.Select(a => a.AddressId).ToList();

                _context.UserAddresses.RemoveRange(userAddresses);

                var addresses = await _context.Addresses
                    .Where(a => addressIds.Contains(a.Id))
                    .ToListAsync(ct);

                _context.Addresses.RemoveRange(addresses);
            }



            _context.VendorProfiles.Remove(vp);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync(ct);
        }


        public async Task SoftDeleteVendorAsync(ulong vendorProfileId, CancellationToken ct = default)
        {
            var vp = await _context.VendorProfiles.FirstOrDefaultAsync(v => v.Id == vendorProfileId, ct);
            if (vp == null)
                throw new KeyNotFoundException("Hồ sơ vendor không tồn tại");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == vp.UserId, ct);
            if (user == null)
                throw new KeyNotFoundException("User không tồn tại");

            // XÓA MỀM = chỉ disable user
            user.Status = UserStatus.Inactive;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync(ct);
        }


        public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
        {
            return await _context.VendorProfiles.AnyAsync(v => v.Slug == slug, ct);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }


        public async Task<bool> ExistsByBusinessRegistrationNumberAsync(string brn, CancellationToken ct)
        {
            return await _context.VendorProfiles
                .AnyAsync(x => x.BusinessRegistrationNumber == brn, ct);
        }

        public async Task<bool> ExistsByTaxCodeAsync(string taxCode, CancellationToken ct)
        {
            return await _context.Users
                .AnyAsync(x => x.TaxCode == taxCode, ct);
        }

        public async Task<List<VendorProfile>> GetAllVerifiedVendorProfilesAsync(CancellationToken cancellationToken = default)
        {
            return await _vendorProfileRepository.GetAllByFilterAsync
                (v => v.VerifiedAt != null && v.VerifiedBy != null, true, cancellationToken);
        }
        
        public async Task<List<Transaction>> GetAllVendorTransactionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.TransactionType == TransactionType.VendorSubscription
                         && t.Status == TransactionStatus.Completed)
                .GroupBy(t => t.UserId)
                .Select(g => g.OrderByDescending(t => t.CreatedAt).First())
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Product>> GetAllProductsToBanAsync(List<ulong> vendorIds, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => vendorIds.Contains(p.VendorId) && p.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        
        public async Task<List<ProductSnapshot>> GetAllProductsToUnBanAsync(List<ulong> vendorIds, CancellationToken cancellationToken = default)
        {
            return await _context.ProductSnapshots
                .Where(ps => vendorIds.Contains(ps.VendorId) && ps.SnapshotType == ProductSnapshotType.SubscriptionBanned)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
