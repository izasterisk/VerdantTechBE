using BLL.DTO;
using BLL.DTO.ProductUpdateRequest;
using DAL.Data;

namespace BLL.Interfaces;

public interface IProductUpdateRequestService
{
    Task<ProductUpdateRequestResponseDTO> CreateProductUpdateRequestAsync(
        ulong userId, 
        ProductUpdateRequestCreateDTO dto, 
        CancellationToken cancellationToken);
    
    Task<ProductUpdateRequestResponseDTO> ProcessProductUpdateRequestAsync(
        ulong staffId, 
        ulong requestId, 
        ProductUpdateRequestUpdateDTO dto, 
        CancellationToken cancellationToken);
    
    Task<string> DeleteProductUpdateRequestAsync(
        ulong userId, 
        ulong requestId, 
        CancellationToken cancellationToken);
    
    Task<PagedResponse<ProductUpdateRequestResponseDTO>> GetAllProductUpdateRequestsAsync(
        int page, 
        int pageSize, 
        ProductRegistrationStatus? status, 
        ulong? vendorId,
        CancellationToken cancellationToken);
    
    Task<PagedResponse<ProductSnapshotResponseDTO>> GetAllProductHistoriesAsync(
        ulong productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}