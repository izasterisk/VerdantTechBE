using AutoMapper;
using BLL.DTO.MediaLink;
using BLL.DTO.VendorCertificate;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services
{
    public class VendorCertificateService : IVendorCertificateService
    {
        private readonly IVendorCertificateRepository _repo;
        private readonly IMapper _mapper;

        public VendorCertificateService(IVendorCertificateRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<VendorCertificateResponseDTO>> GetAllByVendorIdAsync(
            ulong vendorId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var list = await _repo.GetAllByVendorIdAsync(vendorId, page, pageSize, ct);

            var result = _mapper.Map<List<VendorCertificateResponseDTO>>(list);

            return result;
        }

        public async Task<VendorCertificateResponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null)
                return null;

            return _mapper.Map<VendorCertificateResponseDTO>(entity);
        }

        public async Task<List<VendorCertificateResponseDTO>> CreateAsync( VendorCertificateCreateDto dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Danh sách Items không được rỗng.");

            if (addVendorCertificates == null || addVendorCertificates.Count == 0)
                throw new ArgumentException("Phải cung cấp danh sách chứng chỉ (media).");

            if (dto.Items.Count != addVendorCertificates.Count)
                throw new ArgumentException("Số Items và số media không khớp. Mỗi item tương ứng 1 file.");

            var result = new List<VendorCertificateResponseDTO>();

            for (int i = 0; i < dto.Items.Count; i++)
            {
                var item = dto.Items[i];
                var mediaDto = addVendorCertificates[i];

                var entity = new VendorCertificate
                {
                    VendorId = dto.VendorId,
                    CertificationCode = item.CertificationCode,
                    CertificationName = item.CertificationName,
                    Status = VendorCertificateStatus.Pending,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var mediaEntity = new MediaLink
                {
                    ImagePublicId = mediaDto.ImagePublicId,
                    ImageUrl = mediaDto.ImageUrl,
                    Purpose = MediaPurpose.VendorCertificates,
                    SortOrder = mediaDto.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _repo.CreateAsync(dto.VendorId, entity, new[] { mediaEntity }, ct);

                var mapped = _mapper.Map<VendorCertificateResponseDTO>(created);
                mapped.Files.Add(mediaDto);

                result.Add(mapped);
            }

            return result;
        }

        public async Task<VendorCertificateResponseDTO> UpdateAsync( VendorCertificateUpdateDTO dto, List<MediaLinkItemDTO> addVendorCertificates, List<string> removedCertificates, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null)
                throw new KeyNotFoundException($"VendorCertificate {dto.Id} không tồn tại.");

            existing.VendorId = dto.VendorId;
            existing.CertificationCode = dto.CertificationCode;
            existing.CertificationName = dto.CertificationName;
            existing.UpdatedAt = DateTime.UtcNow;

            // Convert thêm MediaLink
            var addMedia = addVendorCertificates?.Select(m => new MediaLink
            {
                ImagePublicId = m.ImagePublicId,
                ImageUrl = m.ImageUrl,
                Purpose = MediaPurpose.VendorCertificates,
                SortOrder = m.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var updated = await _repo.UpdateAsync(
                dto.Id,
                existing,
                addMedia,
                removedCertificates,
                ct
            );

            var mapped = _mapper.Map<VendorCertificateResponseDTO>(updated);

            return mapped;
        }

        public async Task DeleteAsync(ulong id, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing == null)
                throw new KeyNotFoundException($"VendorCertificate {id} không tồn tại.");

            await _repo.DeleteCertificateAsync(existing, ct);
        }

        public async Task<VendorCertificateResponseDTO> ChangeStatusAsync( VendorCertificateChangeStatusDTO dto, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null)
                throw new KeyNotFoundException($"VendorCertificate {dto.Id} không tồn tại.");

            existing.Status = dto.Status;
            existing.RejectionReason = dto.Status == VendorCertificateStatus.Rejected ? dto.RejectionReason : null;
            existing.VerifiedBy = dto.VerifiedBy;
            existing.VerifiedAt = dto.VerifiedBy.HasValue ? DateTime.UtcNow : null;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repo.UpdateAsync(
                dto.Id,
                existing,
                addVendorCertificateFiles: null,
                removeCertificatePublicIds: null,
                ct: ct);

            return _mapper.Map<VendorCertificateResponseDTO>(updated);
        }
    }
}
