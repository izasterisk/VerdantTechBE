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
            var result = _mapper.Map<VendorCertificateResponseDTO>(entity);

            return result;
        }


        public async Task<List<VendorCertificateResponseDTO>> CreateAsync( VendorCertificateCreateDto dto, List<MediaLinkItemDTO> addVendorCertificates, CancellationToken ct = default)
        {
            if (dto.CertificationCode == null || dto.CertificationCode.Count == 0)
                throw new ArgumentException("CertificationCode không được rỗng.");

            if (dto.CertificationName == null || dto.CertificationName.Count == 0)
                throw new ArgumentException("CertificationName không được rỗng.");

            if (addVendorCertificates == null || addVendorCertificates.Count == 0)
                throw new ArgumentException("Danh sách MediaLinkItemDTO không được rỗng.");

            if (dto.CertificationCode.Count != dto.CertificationName.Count ||
                dto.CertificationCode.Count != addVendorCertificates.Count)
                throw new ArgumentException("CertificationCode[], CertificationName[] và Media phải có chung số lượng.");

            var result = new List<VendorCertificateResponseDTO>();

            for (int i = 0; i < dto.CertificationCode.Count; i++)
            {

                var mediaDto = addVendorCertificates[i];

                var entity = new VendorCertificate
                {
                    VendorId = dto.VendorId,
                    CertificationCode = dto.CertificationCode[i],
                    CertificationName = dto.CertificationName[i],
                    Status = VendorCertificateStatus.Pending,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                };

                var mediaEntity = new MediaLink
                {
                    ImagePublicId = mediaDto.ImagePublicId,
                    ImageUrl = mediaDto.ImageUrl,
                    Purpose = MediaPurpose.VendorCertificatesPdf,
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

            if (dto.CertificationCode.Count != dto.CertificationName.Count)
                throw new ArgumentException("CertificationCode[] và CertificationName[] phải bằng nhau.");



            existing.VendorId = dto.VendorId;
            existing.CertificationCode = dto.CertificationCode.First();
            existing.CertificationName = dto.CertificationName.First();
            existing.UpdatedAt = DateTime.UtcNow;


            // Convert thêm MediaLink
            var addMedia = addVendorCertificates?.Select(m => new MediaLink
            {
                ImagePublicId = m.ImagePublicId,
                ImageUrl = m.ImageUrl,
                Purpose = MediaPurpose.VendorCertificatesPdf,
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
            if (dto.Status == VendorCertificateStatus.Rejected &&
       string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                throw new ArgumentException("RejectionReason is required when status is Rejected.");
            }

            // Gọi repo để duyệt / từ chối
            var updated = await _repo.ApproveAsync(
                dto.Id,
                dto.Status,
                dto.VerifiedBy,
                dto.Status == VendorCertificateStatus.Rejected ? dto.RejectionReason : null,
                ct
            );

            if (updated == null)
                throw new KeyNotFoundException($"VendorCertificate {dto.Id} không tồn tại.");
            var result = _mapper.Map<VendorCertificateResponseDTO>(updated);
           
            return result;
        }
    }
}
