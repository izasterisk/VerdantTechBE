using BLL.DTO.VendorProfile;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    using BLL.DTO.MediaLink;
    using BLL.Interfaces;
    using DAL.Data;

    public sealed class VendorProfileService : IVendorProfileService
    {
        private readonly IVendorProfileRepository _profileRepo;
        private readonly IVendorCertificateRepository _certRepo;
        public VendorProfileService(IVendorProfileRepository profileRepo, IVendorCertificateRepository certRepo)
        { _profileRepo = profileRepo; _certRepo = certRepo; }


        public async Task<VendorProfileResponseDTO> CreateAsync(VendorProfileCreateDTO dto, List<MediaLinkItemDTO> addCertificates,CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var profile = new VendorProfile
            {
                CompanyName = dto.CompanyName,
                Slug = dto.Slug,
                BusinessRegistrationNumber = dto.BusinessRegistrationNumber,
                CompanyAddress = dto.CompanyAddress,
                Province = dto.Province,
                District = dto.District,
                Commune = dto.Commune,
                CreatedAt = now,
                UpdatedAt = now
            };
            profile = await _profileRepo.CreateAsync(profile, addVendorCertificateFiles: null, ct);


            // If initial certificates are provided: create one VendorCertificate per file
            if (addCertificates != null && addCertificates.Count > 0)
            {
                foreach (var file in addCertificates)
                {
                    var cert = new VendorCertificate
                    {
                        VendorId = profile.UserId == 0 ? profile.Id : profile.UserId, // adapt to your model
                        CertificationCode = $"AUTO-{now:yyyyMMddHHmmss}",
                        CertificationName = $"{profile.CompanyName} Certificate",
                        Status = VendorCertificateStatus.Pending,
                        UploadedAt = now,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    cert = await _certRepo.CreateAsync(0, cert, addVendorCertificateFiles: null, ct);


                    var media = new MediaLink
                    {
                        Id = cert.Id,
                        OwnerType = MediaOwnerType.VendorCertificates,
                        ImagePublicId = file.ImagePublicId ?? string.Empty,
                        ImageUrl = file.ImageUrl,
                        Purpose = MediaPurpose.None,
                        SortOrder = file.SortOrder,
                        CreatedAt = now
                    };
                    await _certRepo.UpdateAsync(cert.Id, cert, new[] { media }, removeCertificatePublicIds: null, ct);
                }
            }


            return Map(profile);
        }


        public Task<VendorProfileResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
 => _profileRepo.GetByIdAsync(id, ct).ContinueWith(t => t.Result is null ? null : Map(t.Result!), ct);


        public async Task<List<VendorProfileResponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        => (await _profileRepo.GetAllAsync(page, pageSize, ct)).Select(Map).ToList();


        public async Task<VendorProfileResponseDTO> UpdateAsync(ulong id, VendorProfileUpdateDTO dto, CancellationToken ct = default)
        {
            var e = await _profileRepo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("VendorProfile not found");
            e.CompanyName = dto.CompanyName ?? e.CompanyName;
            e.Slug = dto.Slug ?? e.Slug;
            e.BusinessRegistrationNumber = dto.BusinessRegistrationNumber ?? e.BusinessRegistrationNumber;
            e.CompanyAddress = dto.CompanyAddress ?? e.CompanyAddress;
            e.Province = dto.Province ?? e.Province;
            e.District = dto.District ?? e.District;
            e.Commune = dto.Commune ?? e.Commune;
            e.UpdatedAt = DateTime.UtcNow;
            await _profileRepo.UpdateAsync(e, ct);
            return Map(e);
        }


        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var e = await _profileRepo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("VendorProfile not found");
            await _profileRepo.DeleteAsync(e, ct);
        }


        private static VendorProfileResponseDTO Map(VendorProfile e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            CompanyName = e.CompanyName,
            Slug = e.Slug,
            BusinessRegistrationNumber = e.BusinessRegistrationNumber,
            CompanyAddress = e.CompanyAddress,
            Province = e.Province,
            District = e.District,
            Commune = e.Commune,
            VerifiedAt = e.VerifiedAt,
            VerifiedBy = e.VerifiedBy,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}