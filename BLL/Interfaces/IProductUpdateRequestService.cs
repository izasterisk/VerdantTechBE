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
}