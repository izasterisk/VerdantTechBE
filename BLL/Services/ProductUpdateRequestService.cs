using BLL.DTO.ProductUpdateRequest;
using BLL.Interfaces;
using DAL.Data;
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
        var product = await _productUpdateRequestRepository.GetProductByIdAsync(dto.Id, cancellationToken);
        if(product.VendorId != userId)
            throw new UnauthorizedAccessException("Người dùng không có quyền cập nhật sản phẩm này.");
        
        if(dto.ManualFile != null)
        {
            var manualUrl = await FileHelper.UploadFileAsync(dto.ManualFile, "manuals");
            dto.ManualUrls = manualUrl;
        }
    }
}