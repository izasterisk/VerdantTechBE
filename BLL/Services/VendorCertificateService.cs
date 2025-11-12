using BLL.DTO.VendorCertificate;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.Data;
using DAL.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BLL.Interfaces;
using BLL.DTO.MediaLink;

namespace BLL.Services
{
    public sealed class VendorCertificateService : IVendorCertificateService
    {
        private readonly IVendorCertificateRepository _repo;
        private readonly VerdantTechDbContext _db;
        public VendorCertificateService(IVendorCertificateRepository repo, VerdantTechDbContext db)
        { _repo = repo; _db = db; }

        public async Task<IReadOnlyList<VendorCertificateResponseDTO>> CreateAsync(VendorCertificateCreateDTO dto,List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default)
        {
            if (addVendorCertificates is null || addVendorCertificates.Count == 0)
                throw new ValidationException("Cần ít nhất 1 certificate file.");


            var now = DateTime.UtcNow;
            var results = new List<VendorCertificateResponseDTO>(addVendorCertificates.Count);


            await using var tx = await _db.Database.BeginTransactionAsync(ct);


            foreach (var file in addVendorCertificates)
            {
                var e = new VendorCertificate
                {
                    VendorId = dto.VendorId,
                    CertificationCode = dto.CertificationCode,
                    CertificationName = dto.CertificationName,
                    Status = VendorCertificateStatus.Pending,
                    UploadedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };


                e = await _repo.CreateAsync(0, e, addVendorCertificateFiles: null, ct);


                var media = new MediaLink
                {
                    Id = e.Id,
                    OwnerType = MediaOwnerType.VendorCertificates,
                    ImagePublicId = file.ImagePublicId ?? string.Empty,
                    ImageUrl = file.ImageUrl,
                    Purpose = MediaPurpose.None,
                    SortOrder = file.SortOrder,
                    CreatedAt = now
                };
                e = await _repo.UpdateAsync(e.Id, e, new[] { media }, removeCertificatePublicIds: null, ct);


                results.Add(Map(e));
            }


            await tx.CommitAsync(ct);
            return results;
        }

        public async Task<VendorCertificateResponseDTO> UpdateAsync(VendorCertificateUpdateDTO dto,List<MediaLinkItemDTO> addVendorCertificates, List<string> removedCertificates, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(dto.Id, ct) ?? throw new KeyNotFoundException("Certificate not found");


            if (!string.IsNullOrWhiteSpace(dto.CertificationCode)) e.CertificationCode = dto.CertificationCode.Trim();
            if (!string.IsNullOrWhiteSpace(dto.CertificationName)) e.CertificationName = dto.CertificationName.Trim();
            e.UpdatedAt = DateTime.UtcNow;


            IEnumerable<MediaLink>? toAdd = null;
            if (addVendorCertificates != null && addVendorCertificates.Count > 0)
            {
                toAdd = addVendorCertificates.Select(f => new MediaLink
                {
                    Id = e.Id,
                    OwnerType = MediaOwnerType.VendorCertificates,
                    ImagePublicId = f.ImagePublicId ?? string.Empty,
                    ImageUrl = f.ImageUrl,
                    Purpose = MediaPurpose.None,
                    SortOrder = f.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });
            }


            e = await _repo.UpdateAsync(e.Id, e, toAdd, removedCertificates, ct);
            return Map(e);
        }


        public async Task<VendorCertificateResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct);
            return e is null ? null : Map(e);
        }

        public async Task<List<VendorCertificateResponseDTO>> GetAllByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default)
        => (await _repo.GetAllByVendorIdAsync(vendorId, page, pageSize, ct)).Select(Map).ToList();


        public async Task<VendorCertificateResponseDTO> ChangeStatusAsync(VendorCertificateChangeStatusDTO dto, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(dto.Id, ct) ?? throw new KeyNotFoundException("Certificate not found");
            if (dto.Status == VendorCertificateStatus.Rejected && string.IsNullOrWhiteSpace(dto.RejectionReason))
                throw new ValidationException("RejectionReason is required when rejecting.");
            e.Status = dto.Status;
            e.RejectionReason = dto.Status == VendorCertificateStatus.Rejected ? dto.RejectionReason : null;
            e.VerifiedAt = dto.Status == VendorCertificateStatus.Verified ? DateTime.UtcNow : null;
            e.VerifiedBy = dto.VerifiedBy;
            e.UpdatedAt = DateTime.UtcNow;
            e = await _repo.UpdateAsync(e.Id, e, addVendorCertificateFiles: null, removeCertificatePublicIds: null, ct);
            return Map(e);
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Certificate not found");
            await _repo.DeleteCertificateAsync(e, ct);
        }


        private static VendorCertificateResponseDTO Map(VendorCertificate e) => new()
        {
            Id = e.Id,
            VendorId = e.VendorId,
            CertificationCode = e.CertificationCode,
            CertificationName = e.CertificationName,
            Status = e.Status,
            RejectionReason = e.RejectionReason,
            UploadedAt = e.UploadedAt,
            VerifiedAt = e.VerifiedAt,
            VerifiedBy = e.VerifiedBy,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
