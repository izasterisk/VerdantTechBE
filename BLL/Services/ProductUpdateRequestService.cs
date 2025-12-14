using AutoMapper;
using BLL.DTO.ProductUpdateRequest;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class ProductUpdateRequestService : IProductUpdateRequestService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductUpdateRequestRepository _productUpdateRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    public ProductUpdateRequestService(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository,
        IProductUpdateRequestRepository productUpdateRequestRepository, IUserRepository userRepository,
        IMapper mapper)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _productUpdateRequestRepository = productUpdateRequestRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<ProductUpdateRequestResponseDTO> CreateProductUpdateRequestAsync
        (ulong userId, ProductUpdateRequestCreateDTO dto, CancellationToken cancellationToken)
    {
        var product = await _productUpdateRequestRepository.GetProductByIdAsync(dto.ProductId, cancellationToken);
        if(product.VendorId != userId)
            throw new UnauthorizedAccessException("Người dùng không có quyền cập nhật sản phẩm này.");
        
        var productUpdateRequest = _mapper.Map<ProductUpdateRequest>(product);
        _mapper.Map(dto, productUpdateRequest);
        
        if(dto.ManualFile != null)
        {
            var manualUrl = await FileHelper.UploadFileAsync(dto.ManualFile, "manuals");
            dto.ManualUrls = manualUrl;
        }
    }
}