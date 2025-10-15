using System.Threading;
using System.Threading.Tasks;
using BLL.DTO;
using BLL.DTO.ProductRegistration;
using DAL.Data;

namespace BLL.Interfaces
{
    public interface IProductRegistrationService
    {

        //Task<ProductRegistrationReponseDTO> CreateAsync(ProductRegistrationCreateDTO dto, CancellationToken ct = default);
        //Task<ProductRegistrationReponseDTO> UpdateAsync(ProductRegistrationUpdateDTO dto, CancellationToken ct = default);
        //Task<bool> ChangeStatusAsync(ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default);
        //Task<ProductRegistrationReponseDTO> CreateAsync(ProductRegistrationCreateDTO dto, string? manualUrl, string? manualPublicUrl, CancellationToken ct = default);
        //Task<ProductRegistrationReponseDTO> UpdateAsync(ProductRegistrationUpdateDTO dto, string? manualUrl, string? manualPublicUrl, CancellationToken ct = default);
        //Task<bool> ChangeStatusAsync(ulong id, string status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default);

        Task<PagedResponse<ProductRegistrationReponseDTO>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO?> GetByIdAsync(ulong id, CancellationToken ct = default);
        Task<PagedResponse<ProductRegistrationReponseDTO>> GetByVendorAsync(ulong vendorId, int page, int pageSize, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO> CreateAsync(ProductRegistrationCreateDTO dto, string? manualUrl, string? manualPublicUrl, List<DTO.MediaLink.MediaLinkItemDTO> addImages, CancellationToken ct = default);
        Task<ProductRegistrationReponseDTO> UpdateAsync(ProductRegistrationUpdateDTO dto, string? manualUrl, string? manualPublicUrl, List<DTO.MediaLink.MediaLinkItemDTO> addImages, List<string> removed, CancellationToken ct = default);
        Task<bool> ChangeStatusAsync(ulong id, ProductRegistrationStatus status, string? rejectionReason, ulong? approvedBy, CancellationToken ct = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken ct = default);
    }
}
