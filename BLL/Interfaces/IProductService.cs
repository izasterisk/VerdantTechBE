using BLL.DTO.Product;
using BLL.DTO.ProductRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProductService
    {
        Task<ProductRegistrationReponseDTO> ProductRegistrationAsync(ulong currentUserId, ProductRegistrationCreateDTO requestDTO, CancellationToken cancellationToken = default);  
        Task<ProductResponseDTO?> GetProductByIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductRegistrationReponseDTO?>> GetAllProductByVendorIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductResponseDTO?>> GetAllProductByCategoryIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductResponseDTO>> GetAllProductAsync(CancellationToken cancellationToken = default);
        Task<ProductResponseDTO> UpdateProductAsync(ulong id, ProductUpdateDTO dto, CancellationToken cancellationToken = default);
    }
}
