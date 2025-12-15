using BLL.DTO.ProductUpdateRequest;

namespace BLL.Interfaces;

public interface IProductUpdateRequestService
{
    Task<ProductUpdateRequestResponseDTO> CreateProductUpdateRequestAsync(
        ulong userId, 
        ProductUpdateRequestCreateDTO dto, 
        CancellationToken cancellationToken);
}