using System.Threading;
using System.Threading.Tasks;
using BLL.DTO;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductRegistration;
using DAL.Data;

namespace BLL.Interfaces
{
    public interface IProductRegistrationService
    {

        Task<PagedResponse<ProductRegistrationReponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<PagedResponse<ProductRegistrationReponseDTO>> GetByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO> CreateAsync(ProductRegistrationCreateDTO dto, string? manualUrl, string? manualPublicUrl, List<DTO.MediaLink.MediaLinkItemDTO> addImages, List<MediaLinkItemDTO> addCertificates, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO> UpdateAsync(ProductRegistrationUpdateDTO dto, string? manualUrl, string? manualPublicUrl, List<DTO.MediaLink.MediaLinkItemDTO> addImages, List<MediaLinkItemDTO> addCertificates, List<string> removedImages, List<string> removedCertificates, CancellationToken ct = default);
        Task<bool> ChangeStatusAsync(ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
    }
}
