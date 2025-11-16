using BLL.DTO.MediaLink;
using BLL.DTO.VendorProfile;
using BLL.Interfaces.Infrastructure;
using DAL.Data.Models;
using DAL.IRepository;
using BLL.Interfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;    // thêm để bắt DbUpdateException
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BLL.Helpers.Auth;

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
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email không được rỗng.", nameof(dto.Email));

            // ===== 1. Validate phần chứng chỉ =====

            var codes = dto.CertificationCode ?? new List<string>();
            var names = dto.CertificationName ?? new List<string>();
            var mediaList = addVendorCertificateFiles?.ToList() ?? new List<MediaLink>();

            if (codes.Count == 0)
                throw new ArgumentException("CertificationCode không được rỗng.");

            if (names.Count == 0)
                throw new ArgumentException("CertificationName không được rỗng.");

            if (mediaList.Count == 0)
                throw new ArgumentException("Danh sách file chứng chỉ không được rỗng.");

            if (codes.Count != names.Count || codes.Count != mediaList.Count)
                throw new ArgumentException("CertificationCode[], CertificationName[] và files phải có chung số lượng.");


            // ===== 2. Tạo User =====
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password không được rỗng.", nameof(dto.Password));
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = AuthUtils.HashPassword(dto.Password),             
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                TaxCode = dto.TaxCode,
                Role = UserRole.Vendor,                  
                Status = UserStatus.Active,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                user = await _userRepository.CreateUserWithTransactionAsync(user, ct);
                user.Role = UserRole.Vendor;
                user.Status = UserStatus.Active;
                await _userRepository.UpdateUserWithTransactionAsync(user, ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("idx_email") == true)
            {
                // Duplicate email
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống.", ex);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("idx_tax_code") == true)
            {
                // Duplicate tax code
                throw new InvalidOperationException("Mã số thuế đã tồn tại trong hệ thống.", ex);
            }

            // ===== 3. Tạo VendorProfile (KHÔNG gắn file ở đây) =====

            var baseSlug = string.IsNullOrWhiteSpace(dto.Slug) ? dto.CompanyName : dto.Slug;

            // FIX: tránh duplicate idx_slug bằng cách auto thêm suffix random
            var safeSlug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";

            var vendorProfile = new VendorProfile
            {
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                Slug = safeSlug,
                BusinessRegistrationNumber = dto.BusinessRegistrationNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            VendorProfile createdProfile;
            try
            {
                createdProfile = await _vendorProfileRepository.CreateAsync(
                    vendorProfile,
                    addVendorCertificateFiles: null,
                    ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("idx_slug") == true)
            {
                // Trong TH rất hiếm vẫn trùng (cực ít) thì báo lỗi rõ ràng
                throw new InvalidOperationException("Slug công ty đã tồn tại. Vui lòng thử lại.", ex);
            }

            createdProfile.User = user;

            // ===== 4. Tạo Address + UserAddress nếu có =====

            if (!string.IsNullOrWhiteSpace(dto.CompanyAddress) ||
                !string.IsNullOrWhiteSpace(dto.Province) ||
                !string.IsNullOrWhiteSpace(dto.District) ||
                !string.IsNullOrWhiteSpace(dto.Commune))
            {
                var address = new Address
                {
                    LocationAddress = dto.CompanyAddress,
                    Province = dto.Province,
                    District = dto.District,
                    Commune = dto.Commune,
                    ProvinceCode = string.Empty,
                    DistrictCode = string.Empty,
                    CommuneCode = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _addressRepository.CreateUserAddressAsync(user.Id, address, ct);
            }

            // ===== 5. Tạo VendorCertificate + Media cho từng chứng chỉ =====

            for (int i = 0; i < codes.Count; i++)
            {
                var certEntity = new VendorCertificate
                {
                    VendorId = user.Id,                       // auto gán VendorId theo user mới tạo
                    CertificationCode = codes[i],
                    CertificationName = names[i],
                    Status = VendorCertificateStatus.Pending,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var media = mediaList[i];

                // FIX: dùng 1 Purpose an toàn, đang được DB chấp nhận
                // (giả định MediaPurpose.VendorCertificates là value đã dùng trước đó)
                media.Purpose = MediaPurpose.VendorCertificatesPdf;

                media.SortOrder = (ushort)i;
                media.CreatedAt = DateTime.UtcNow;
                media.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _vendorCertificateRepository.CreateAsync(
                        user.Id,
                        certEntity,
                        new[] { media },
                        ct);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("purpose") == true)
                {
                    // Nếu vẫn lỗi purpose thì ném ra message rõ ràng
                    throw new InvalidOperationException("Giá trị purpose của media không hợp lệ hoặc không khớp DB.", ex);
                }
            }

            // ===== 6. Trả về VendorProfileResponseDTO (map kèm address, media url) =====
            return await MapToResponseWithAddressAsync(createdProfile, ct);
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
            var vp = await _vendorProfileRepository.GetByIdAsync(dto.Id, ct);
            if (vp == null)
                throw new KeyNotFoundException("VendorProfile không tồn tại");

            vp.CompanyName = dto.CompanyName;
            vp.Slug = dto.Slug ?? vp.Slug; // không bắt buộc đổi slug, tránh phát sinh slug random không cần thiết
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
                if (!string.IsNullOrWhiteSpace(dto.CompanyAddress) ||
                    !string.IsNullOrWhiteSpace(dto.Province) ||
                    !string.IsNullOrWhiteSpace(dto.District) ||
                    !string.IsNullOrWhiteSpace(dto.Commune))
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
                            Province = dto.Province,
                            District = dto.District,
                            Commune = dto.Commune,
                            ProvinceCode = string.Empty,
                            DistrictCode = string.Empty,
                            CommuneCode = string.Empty,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _addressRepository.CreateUserAddressAsync(user.Id, newAddr, ct);
                    }
                }
            }
        }


        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException("VendorProfile không tồn tại");

            await _vendorProfileRepository.DeleteAsync(vp, ct);
        }

        public async Task SoftDeleteAccountAsync(ulong userId, CancellationToken ct = default)
        {
            await _vendorProfileRepository.SoftDeleteAccountAsync(userId, ct);
        }


        public async Task ApproveAsync(VendorProfileApproveDTO dto, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(dto.Id, ct)
                     ?? throw new KeyNotFoundException("VendorProfile không tồn tại");

            var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct)
                      ?? throw new KeyNotFoundException("User không tồn tại");

            var certs = await _vendorCertificateRepository.GetAllByVendorIdAsync(
                vp.UserId, 1, int.MaxValue, ct);

            var now = DateTime.UtcNow;

            // 1. User: verify = true
            user.IsVerified = true;
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
            // FIX: không truyền string rỗng vào password
            // ở đây tạm truyền "********" để tránh ArgumentException trong EmailSender
            await _emailSender.SendVendorProfileVerifiedEmailAsync(
                user.Email!,
                user.FullName ?? user.Email!,
                user.Email!,
                user.PasswordHash,
                ct);
        }


        public async Task RejectAsync(VendorProfileRejectDTO dto, CancellationToken ct = default)
        {
            var vp = await _vendorProfileRepository.GetByIdAsync(dto.Id, ct)
                     ?? throw new KeyNotFoundException("VendorProfile không tồn tại");

            var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct)
                      ?? throw new KeyNotFoundException("User không tồn tại");

            var certs = await _vendorCertificateRepository.GetAllByVendorIdAsync(
                vp.UserId, 1, int.MaxValue, ct);

            var now = DateTime.UtcNow;

            // 1. User: mark not verified
            user.IsVerified = false;
            user.VerificationSentAt = now;
            user.UpdatedAt = now;
            await _userRepository.UpdateUserWithTransactionAsync(user, ct);

            // 2. VendorProfile
            vp.VerifiedAt = now;
            vp.VerifiedBy = dto.VerifiedBy;
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


        //private async Task<VendorProfileResponseDTO> MapToResponseWithAddressAsync(
        //    VendorProfile vp,
        //    CancellationToken ct)
        //{
        //    var user = await _userRepository.GetUserWithAddressesByIdAsync(vp.UserId, ct);

        //    Address? address = null;
        //    if (user?.UserAddresses != null && user.UserAddresses.Any())
        //    {
        //        var ua = user.UserAddresses
        //            .Where(x => !x.IsDeleted)
        //            .OrderByDescending(x => x.CreatedAt)
        //            .FirstOrDefault();

        //        address = ua?.Address;
        //    }

        //    return new VendorProfileResponseDTO
        //    {
        //        Id = vp.Id,
        //        UserId = vp.UserId,

        //        Email = user?.Email,
        //        FullName = user?.FullName,
        //        PhoneNumber = user?.PhoneNumber,
        //        TaxCode = user?.TaxCode,
        //        AvatarUrl = user?.AvatarUrl,   // URL avatar đầy đủ
        //        Status = user?.Status ?? UserStatus.Inactive,

        //        CompanyName = vp.CompanyName,
        //        Slug = vp.Slug,
        //        BusinessRegistrationNumber = vp.BusinessRegistrationNumber,

        //        CompanyAddress = address?.LocationAddress,
        //        Province = address?.Province,
        //        District = address?.District,
        //        Commune = address?.Commune,

        //        // Files: trả cả ImageUrl & ImagePublicId
        //        Files = (vp.MediaLinks ?? new List<MediaLink>())
        //            .Select(m => new MediaLinkItemDTO
        //            {
        //                Id = m.Id,
        //                ImageUrl = m.ImageUrl,           // URL đầy đủ cho FE
        //                ImagePublicId = m.ImagePublicId,
        //                Purpose = m.Purpose.ToString(),
        //                SortOrder = m.SortOrder
        //            }).ToList(),

        //        VerifiedAt = vp.VerifiedAt,
        //        VerifiedBy = vp.VerifiedBy,
        //        CreatedAt = vp.CreatedAt,
        //        UpdatedAt = vp.UpdatedAt
        //    };
        //}

        private async Task<VendorProfileResponseDTO> MapToResponseWithAddressAsync( VendorProfile vp, CancellationToken ct)
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

                CompanyAddress = address?.LocationAddress,
                Province = address?.Province,
                District = address?.District,
                Commune = address?.Commune,

                // Files: trả ra URL các file chứng chỉ
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


    }
}