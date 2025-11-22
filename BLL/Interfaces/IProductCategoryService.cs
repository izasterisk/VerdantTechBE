using BLL.DTO.ProductCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryResponseDTO> CreateProductCategoryAsync( ProductCategoryCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ProductCategoryResponseDTO> CreateSubProductCategoryAsync(ProductCategoryCreateDTO dto, CancellationToken cancellationToken = default);
        Task<ProductCategoryResponseDTO?> GetProductCategoryByIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProductCategoryResponseDTO>> GetAllProductCategoryAsync( CancellationToken cancellationToken = default);
        Task<ProductCategoryResponseDTO> UpdateProductCategoryAsync(ulong id, ProductCategoryUpdateDTO dto, CancellationToken cancellationToken = default);
    }
}
