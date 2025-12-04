using BLL.DTO.MediaLink;
using BLL.DTO.VendorProfile;
using BLL.Interfaces.Infrastructure;
using DAL.Data.Models;
using DAL.IRepository;
using BLL.Interfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.Helpers.Auth;
using BLL.Helpers;

namespace BLL.Service
{
    public class VendorProfileService : IVendorProfileService
    {
        private readonly IVendorProfileRepository _vendorProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVendorCertificateRepository _vendorCertificateRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IEmailSender _emailSender;

        public VendorProfileService(
            IVendorProfileRepository vendorProfileRepository,
            IUserRepository userRepository,
            IVendorCertificateRepository vendorCertificateRepository,
            IAddressRepository addressRepository,
            IEmailSender emailSender)
        {
            _vendorProfileRepository = vendorProfileRepository;
            _userRepository = userRepository;
            _vendorCertificateRepository = vendorCertificateRepository;
            _addressRepository = addressRepository;
            _emailSender = emailSender;
        }

        

        public async Task<VendorProfileResponseDTO> CreateAsync(
            VendorProfileCreateDTO dto,
            IEnumerable<MediaLink>? addVendorCertificateFiles,
            CancellationToken ct = default)
        {
            // 1. Validate input cơ bản
            ValidateEmail(dto.Email);
            ValidatePassword(dto.Password);

            // 2. Validate danh sách chứng chỉ + file
            var (codes, names, mediaList) = ValidateAndNormalizeCertificates(dto, addVendorCertificateFiles);

            // 3. Tạo user vendor
            var user = await CreateVendorUserAsync(dto, ct);

            // 4. Tạo VendorProfile với slug auto-generate
            //var vendorProfile = await CreateVendorProfileAsync(user, dto, ct);

            var existingProfile = await _vendorProfileRepository.GetByUserIdAsync(user.Id, ct);

            VendorProfile vendorProfile;

            if (existingProfile != null)
            {
                var oldCompanyName = existingProfile.CompanyName;

                existingProfile.CompanyName = dto.CompanyName;
                existingProfile.BusinessRegistrationNumber = dto.BusinessRegistrationNumber;

                if (!string.Equals(oldCompanyName, dto.CompanyName, StringComparison.OrdinalIgnoreCase))
                {
                    existingProfile.Slug = await GenerateUniqueSlugAsync(dto.CompanyName, ct);
                }

                existingProfile.VerifiedAt = null;
                existingProfile.VerifiedBy = null;
                existingProfile.Notes = null;
                existingProfile.UpdatedAt = DateTime.UtcNow;

                await _vendorProfileRepository.UpdateAsync(existingProfile, ct);
                vendorProfile = existingProfile;

                await _vendorCertificateRepository.DeleteAllByVendorIdAsync(user.Id, ct);
            }
            else
            {
                vendorProfile = await CreateVendorProfileAsync(user, dto, ct);
            }




            // 5. Tạo Address (nếu có)
            await CreateUserAddressIfNeededAsync(user.Id, dto, ct);

            // 6. Tạo VendorCertificate + Media cho từng chứng chỉ
            await CreateVendorCertificatesAsync(user.Id, codes, names, mediaList, ct);

            // 7. Email: gửi thông báo đăng ký thành công – đang chờ duyệt
            await _emailSender.SendVendorProfileSubmittedEmailAsync(
                user.Email!,
                user.FullName ?? user.Email!,
                ct
            );

            //8. Trả về kết quả
            vendorProfile.User = user;
            return await MapToResponseWithAddressAsync(vendorProfile, ct);
        }

        public async Task<VendorProfileResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(id, ct);
            if (vp == null) return null;

            return await MapToResponseWithAddressAsync(vp, ct);
        }

        public async Task<VendorProfileResponseDTO?> GetByUserIdAsync(ulong userId, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByUserIdAsync(userId, ct);
            if (vp == null) return null;

            return await MapToResponseWithAddressAsync(vp, ct);
        }

        public async Task<List<VendorProfileResponseDTO>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var list = await _vendorProfileRepository.GetAllAsync(page, pageSize, ct);
            var result = new List<VendorProfileResponseDTO>();

            foreach (var vp in list)
            {
                result.Add(await MapToResponseWithAddressAsync(vp, ct));
            }

            return result;
        }

        public async Task UpdateAsync(VendorProfileUpdateDTO dto, CancellationToken ct = default)
        {
            // 1. VendorProfile
            var vp = await _vendorProfileRepository.GetByIdAsync(dto.Id, ct)
                     ?? throw new InvalidOperationException("Hồ sơ vendor không tồn tại.");

            //vp.CompanyName = dto.CompanyName;
            // vp.Slug = dto.Slug ?? vp.Slug;
            var oldCompanyName = vp.CompanyName;

            vp.CompanyName = dto.CompanyName;

            // Nếu đổi tên → đổi slug
            if (!string.Equals(oldCompanyName, dto.CompanyName, StringComparison.OrdinalIgnoreCase))
            {
                vp.Slug = await GenerateUniqueSlugAsync(dto.CompanyName, ct);
            }

            vp.BusinessRegistrationNumber = dto.BusinessRegistrationNumber;
            vp.UpdatedAt = DateTime.UtcNow;

            await _vendorProfileRepository.UpdateAsync(vp, ct);

            // 2. User
            var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
                if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
                if (!string.IsNullOrWhiteSpace(dto.TaxCode)) user.TaxCode = dto.TaxCode;

                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserWithTransactionAsync(user, ct);

                // 3. Address
                if (HasAddressInfo(dto.CompanyAddress, dto.Province, dto.District, dto.Commune))
                {
                    await CreateOrUpdateUserAddressAsync(user, dto, ct);
                }
            }
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(id, ct)
                     ?? throw new InvalidOperationException("Hồ sơ vendor không tồn tại.");

            await _vendorProfileRepository.HardDeleteVendorAsync(id, ct);
           
        }

        public async Task SoftDeleteAccountAsync(ulong vendorProfileId, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(vendorProfileId, ct)
                     ?? throw new InvalidOperationException("Hồ sơ vendor không tồn tại.");

            await _vendorProfileRepository.SoftDeleteVendorAsync(vendorProfileId, ct);
        }


        public async Task ApproveAsync(VendorProfileApproveDTO dto, CancellationToken ct = default)
        {
            var (vp, user, certs) = await GetVendorWithUserAndCertificatesAsync(dto.Id, ct);

            var now = DateTime.UtcNow;

            user.IsVerified = true;
            user.Status = UserStatus.Active;
            user.Role = UserRole.Vendor;

            user.VerificationSentAt = now;
            user.UpdatedAt = now;
            await _userRepository.UpdateUserWithTransactionAsync(user, ct);

            // 2. VendorProfile
            vp.VerifiedAt = now;
            vp.VerifiedBy = dto.VerifiedBy;
            vp.UpdatedAt = now;
            await _vendorProfileRepository.UpdateAsync(vp, ct);

            // 3. Certificates: Verified
            foreach (var c in certs)
            {
                await _vendorCertificateRepository.ApproveAsync(
                    c.Id,
                    VendorCertificateStatus.Verified,
                    dto.VerifiedBy,
                    null,
                    ct);
            }

            // 4. Email
            await _emailSender.SendVendorProfileVerifiedEmailAsync(
                user.Email!,
                user.FullName ?? user.Email!,
                user.Email!,
                user.PasswordHash,
                ct);
            Console.WriteLine("Sending email to: " + user.Email);

        }

        public async Task RejectAsync(VendorProfileRejectDTO dto, CancellationToken ct = default)
        {
            var (vp, user, certs) = await GetVendorWithUserAndCertificatesAsync(dto.Id, ct);

            var now = DateTime.UtcNow;

            // 1. User: mark not verified
            user.IsVerified = false;
            user.Status = UserStatus.Inactive;
            user.Role = UserRole.Vendor;
            user.VerificationSentAt = now;
            user.UpdatedAt = now;
            await _userRepository.UpdateUserWithTransactionAsync(user, ct);

            // 2. VendorProfile
            vp.VerifiedAt = now;
            vp.VerifiedBy = dto.VerifiedBy;
            vp.Notes = dto.RejectionReason;
            vp.UpdatedAt = now;
            await _vendorProfileRepository.UpdateAsync(vp, ct);

            // 3. Certificates: Rejected + reason
            foreach (var c in certs)
            {
                await _vendorCertificateRepository.ApproveAsync(
                    c.Id,
                    VendorCertificateStatus.Rejected,
                    dto.VerifiedBy,
                    dto.RejectionReason,
                    ct);
            }

            // 4. Email
            await _emailSender.SendVendorProfileRejectedEmailAsync(
                user.Email!,
                user.FullName ?? user.Email!,
                dto.RejectionReason,
                ct);
        }

        

        private static void ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                //throw new ArgumentException("Email không được rỗng.", nameof(email));
                throw new InvalidOperationException("Email không được để trống.");

        }

        private static void ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                //throw new ArgumentException("Password không được rỗng.", nameof(password));
                throw new InvalidOperationException("Mật khẩu không được để trống.");

        }

        private static (List<string> Codes, List<string> Names, List<MediaLink> MediaList)
            ValidateAndNormalizeCertificates(
                VendorProfileCreateDTO dto,
                IEnumerable<MediaLink>? addVendorCertificateFiles)
        {
            var codes = dto.CertificationCode ?? new List<string>();
            var names = dto.CertificationName ?? new List<string>();
            var mediaList = addVendorCertificateFiles?.ToList() ?? new List<MediaLink>();

            if (codes.Count == 0)
                throw new InvalidOperationException("Thiếu mã chứng chỉ.");

            if (names.Count == 0)
                throw new InvalidOperationException("Thiếu tên chứng chỉ.");

            if (mediaList.Count == 0)
                throw new InvalidOperationException("Thiếu file chứng chỉ.");

            if (codes.Count != names.Count || codes.Count != mediaList.Count)
                throw new InvalidOperationException("Số lượng mã – tên – file chứng chỉ không khớp.");

            return (codes, names, mediaList);
        }

        private static bool HasAddressInfo(
            string? companyAddress,
            string? province,
            string? district,
            string? commune)
        {
            return !string.IsNullOrWhiteSpace(companyAddress)
                   || !string.IsNullOrWhiteSpace(province)
                   || !string.IsNullOrWhiteSpace(district)
                   || !string.IsNullOrWhiteSpace(commune);
        }


        private async Task<User> CreateVendorUserAsync(
            VendorProfileCreateDTO dto,
            CancellationToken ct)
        {
            var existingUser = await _vendorProfileRepository.GetUserByEmailAsync(dto.Email, ct);
        
            
            if (existingUser != null)
            {

                if (existingUser != null &&
                (existingUser.Status == UserStatus.Active || existingUser.IsVerified))
                throw new InvalidOperationException("Email này đã được đăng ký và đang hoạt động.");

                if (await _vendorProfileRepository.ExistsByBusinessRegistrationNumberAsync(
                    dto.BusinessRegistrationNumber, ct))
                throw new InvalidOperationException("Mã số đăng ký kinh doanh đã tồn tại.");

                if (!string.IsNullOrWhiteSpace(dto.TaxCode) &&
                await _vendorProfileRepository.ExistsByTaxCodeAsync(dto.TaxCode, ct))
                throw new InvalidOperationException("Mã số thuế đã tồn tại.");

            
                // Nếu user chưa active → CHO PHÉP ghi đè
                existingUser.FullName = dto.FullName ?? existingUser.FullName;
                existingUser.PhoneNumber = dto.PhoneNumber ?? existingUser.PhoneNumber;
                existingUser.TaxCode = dto.TaxCode ?? existingUser.TaxCode;
                existingUser.IsVerified = false;
                existingUser.Status = UserStatus.Inactive;

                existingUser.Role = UserRole.Vendor;

                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUserWithTransactionAsync(existingUser, ct);
                return existingUser;
            }

            // 2. Nếu chưa tồn tại → tạo mới
            var newUser = new User
            {
                Email = dto.Email,
                PasswordHash = AuthUtils.HashPassword(dto.Password),
                FullName = dto.FullName ?? "",
                PhoneNumber = dto.PhoneNumber,
                TaxCode = dto.TaxCode,

                Role = UserRole.Vendor,
                Status = UserStatus.Inactive,

                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            //user = await _userRepository.CreateUserWithTransactionAsync(user, ct);
            //return user;
            newUser = await _userRepository.CreateUserWithTransactionAsync(newUser, ct);
            newUser.Role = UserRole.Vendor;
            newUser.Status = UserStatus.Inactive;
            newUser.IsVerified = false;
            newUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserWithTransactionAsync(newUser, ct);

            return newUser;
        }


        private async Task<VendorProfile> CreateVendorProfileAsync(
            User user,
            VendorProfileCreateDTO dto,
            CancellationToken ct)
        {
            //var safeSlug = GenerateUniqueSlugAsync(dto.CompanyName);
            var safeSlug = await GenerateUniqueSlugAsync(dto.CompanyName, ct);

            var vendorProfile = new VendorProfile
            {
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                Slug = safeSlug,
                BusinessRegistrationNumber = dto.BusinessRegistrationNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                return await _vendorProfileRepository.CreateAsync(
                    vendorProfile,
                    addVendorCertificateFiles: null,
                    ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("idx_slug") == true)
            {
                throw new InvalidOperationException("Slug công ty đã tồn tại. Vui lòng thử lại.", ex);
            }
        }

        private async Task CreateUserAddressIfNeededAsync(
            ulong userId,
            VendorProfileCreateDTO dto,
            CancellationToken ct)
        {
            if (!HasAddressInfo(dto.CompanyAddress, dto.Province, dto.District, dto.Commune))
                return;

            var address = new Address
            {
                LocationAddress = dto.CompanyAddress,
                Province = dto.Province ?? "",
                District = dto.District ?? "",
                Commune = dto.Commune ?? "",
                ProvinceCode = string.Empty,
                DistrictCode = string.Empty,
                CommuneCode = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _addressRepository.CreateUserAddressAsync(userId, address, ct);
        }

        private async Task CreateOrUpdateUserAddressAsync(
            User user,
            VendorProfileUpdateDTO dto,
            CancellationToken ct)
        {
            var currentUserAddress = user.UserAddresses?
                .Where(ua => !ua.IsDeleted)
                .OrderByDescending(ua => ua.CreatedAt)
                .FirstOrDefault();

            if (currentUserAddress?.Address != null)
            {
                var addr = currentUserAddress.Address;
                addr.LocationAddress = dto.CompanyAddress ?? addr.LocationAddress;
                addr.Province = dto.Province ?? addr.Province;
                addr.District = dto.District ?? addr.District;
                addr.Commune = dto.Commune ?? addr.Commune;
                addr.UpdatedAt = DateTime.UtcNow;

                await _addressRepository.UpdateAddressAsync(addr, ct);
            }
            else
            {
                var newAddr = new Address
                {
                    LocationAddress = dto.CompanyAddress,
                    Province = dto.Province ?? "",
                    District = dto.District ?? "",
                    Commune = dto.Commune ?? "",
                    ProvinceCode = string.Empty,
                    DistrictCode = string.Empty,
                    CommuneCode = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _addressRepository.CreateUserAddressAsync(user.Id, newAddr, ct);
            }
        }

        private async Task CreateVendorCertificatesAsync(
            ulong vendorId,
            List<string> codes,
            List<string> names,
            List<MediaLink> mediaList,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            for (int i = 0; i < codes.Count; i++)
            {
                var certEntity = new VendorCertificate
                {
                    VendorId = vendorId,
                    CertificationCode = codes[i],
                    CertificationName = names[i],
                    Status = VendorCertificateStatus.Pending,
                    UploadedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var media = mediaList[i];
                media.Purpose = MediaPurpose.VendorCertificatesPdf; // Purpose an toàn, đã được DB chấp nhận
                media.SortOrder = (ushort)i;
                media.CreatedAt = now;
                media.UpdatedAt = now;

                try
                {
                    await _vendorCertificateRepository.CreateAsync(
                        vendorId,
                        certEntity,
                        new[] { media },
                        ct);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("purpose") == true)
                {
                    throw new InvalidOperationException("Giá trị purpose của media không hợp lệ hoặc không khớp DB.", ex);
                }
            }
        }

      

        private async Task<(VendorProfile vp, User user, List<VendorCertificate> certs)>
            GetVendorWithUserAndCertificatesAsync(ulong vendorProfileId, CancellationToken ct)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(vendorProfileId, ct)
                     ?? throw new InvalidOperationException("Không tìm thấy hồ sơ nhà cung cấp.");

            var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct)
                      ?? throw new InvalidOperationException("Không tìm thấy người dùng.");

            var certs = await _vendorCertificateRepository
                .GetAllByVendorIdAsync(vp.UserId, 1, int.MaxValue, ct)
                ?? new List<VendorCertificate>();

            return (vp, user, certs);
        }

        private async Task<VendorProfileResponseDTO> MapToResponseWithAddressAsync(
            VendorProfile vp,
            CancellationToken ct)
        {
            // 1. Lấy User + Address
            var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct);

            Address? address = null;
            if (user?.UserAddresses != null && user.UserAddresses.Any())
            {
                var ua = user.UserAddresses
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                address = ua?.Address;
            }

            // 2. Lấy toàn bộ VendorCertificate của vendor + kèm MediaLinks
            var certs = await _vendorCertificateRepository
                .GetAllByVendorIdAsync(vp.UserId, 1, int.MaxValue, ct)
                ?? new List<VendorCertificate>();

            // 3. Gộp media từ tất cả certificates lại
            var mediaFiles = certs
                .Where(c => c.MediaLinks != null && c.MediaLinks.Any())
                .SelectMany(c => c.MediaLinks!)
                .OrderBy(m => m.SortOrder)
                .ToList();

            return new VendorProfileResponseDTO
            {
                Id = vp.Id,
                UserId = vp.UserId,

                Email = user?.Email,
                FullName = user?.FullName,
                PhoneNumber = user?.PhoneNumber,
                TaxCode = user?.TaxCode,
                AvatarUrl = user?.AvatarUrl,
                Status = user?.Status ?? UserStatus.Inactive,

                CompanyName = vp.CompanyName,
                Slug = vp.Slug,
                BusinessRegistrationNumber = vp.BusinessRegistrationNumber,
                Notes = vp.Notes,

                CompanyAddress = address?.LocationAddress,
                Province = address?.Province,
                District = address?.District,
                Commune = address?.Commune,

                Files = mediaFiles
                    .Select(m => new MediaLinkItemDTO
                    {
                        Id = m.Id,
                        ImageUrl = m.ImageUrl,
                        ImagePublicId = m.ImagePublicId,
                        Purpose = m.Purpose.ToString(),
                        SortOrder = m.SortOrder
                    }).ToList(),

                VerifiedAt = vp.VerifiedAt,
                VerifiedBy = vp.VerifiedBy,
                CreatedAt = vp.CreatedAt,
                UpdatedAt = vp.UpdatedAt
            };
        }

        private async Task<string> GenerateUniqueSlugAsync(string companyName, CancellationToken ct)
        {
            var baseSlug = Utils.GenerateSlug(companyName);

            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "vendor";

            var slug = baseSlug;
            int attempt = 1;

            while (await _vendorProfileRepository.ExistsBySlugAsync(slug, ct))
            {
                slug = $"{baseSlug}-{attempt++}";
            }

            return slug;
        }
    }
}
