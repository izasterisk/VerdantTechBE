using BLL.DTO.ProductUpdateRequest;
using BLL.Interfaces;
using DAL.IRepository;

namespace BLL.Services;

public class ProductUpdateRequestService : IProductUpdateRequestService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductUpdateRequestRepository _productUpdateRequestRepository;
    private readonly IUserRepository _userRepository;
    
    public ProductUpdateRequestService(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository,
        IProductUpdateRequestRepository productUpdateRequestRepository, IUserRepository userRepository)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _productUpdateRequestRepository = productUpdateRequestRepository;
        _userRepository = userRepository;
    }
    
    public async Task<ProductUpdateRequestResponseDTO> CreateProductUpdateRequestAsync
        (ulong userId, ProductUpdateRequestCreateDTO dto, CancellationToken cancellationToken)
    {
        if(dto.DimensionsCm != null)
        {
            
        }
        if(dto.Specifications != null)
        {
            
        }
    }
}