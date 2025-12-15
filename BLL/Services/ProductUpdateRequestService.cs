using AutoMapper;
using BLL.DTO.MediaLink;
using BLL.DTO.ProductUpdateRequest;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
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
    private readonly ICloudinaryService _cloudinaryService;
    
    public ProductUpdateRequestService(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository,
        IProductUpdateRequestRepository productUpdateRequestRepository, IUserRepository userRepository,
        IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
        _productUpdateRequestRepository = productUpdateRequestRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }
    
    public async Task<ProductUpdateRequestResponseDTO> CreateProductUpdateRequestAsync
        (ulong userId, ProductUpdateRequestCreateDTO dto, CancellationToken cancellationToken)
    {
        var product = await _productUpdateRequestRepository.GetProductByIdAsync(dto.ProductId, cancellationToken);
        if(product.VendorId != userId)
            throw new UnauthorizedAccessException("Người dùng không có quyền cập nhật sản phẩm này.");
        if(await _productUpdateRequestRepository.IsThereAnyPendingRequestAsync(dto.ProductId,cancellationToken))
            throw new InvalidOperationException("Đã có yêu cầu cập nhật sản phẩm đang chờ xử lý.");
        
        var productSnapshot = _mapper.Map<ProductSnapshot>(product);
        _mapper.Map(dto, productSnapshot);
        productSnapshot.SnapshotType = ProductSnapshotType.Proposed;
        
        if(dto.ProductName != null)
            productSnapshot.Slug = Utils.GenerateSlug(dto.ProductName);
        if(dto.ManualFile != null)
        {
            var manualResult = await Utils.UploadManualFileAsync(
                _cloudinaryService,
                dto.ManualFile,
                "product-update-requests/manuals",
                cancellationToken
            );
            if (manualResult != null)
            {
                productSnapshot.ManualUrls = manualResult.Value.Url;
                productSnapshot.PublicUrl = manualResult.Value.PublicUrl;
            }
        }
        var images = new List<MediaLink>();
        if(dto.Images != null && dto.Images.Count > 0)
        {
            var uploadedImages = await Utils.UploadImagesAsync(
                _cloudinaryService,
                dto.Images,
                "product-update-requests/images",
                "productImage",
                cancellationToken
            );
            foreach (var img in uploadedImages)
            {
                images.Add(new MediaLink
                {
                    OwnerType = MediaOwnerType.ProductSnapshot,
                    // OwnerId
                    ImageUrl = img.ImageUrl,
                    ImagePublicId = img.ImagePublicId,
                    Purpose = MediaPurpose.None,
                    SortOrder = img.SortOrder,
                });
            }
        }
        var request = new ProductUpdateRequest()
        {
            // ProductSnapshotId 
            ProductId = dto.ProductId,
            Status = ProductRegistrationStatus.Pending,
            // RejectionReason
            // ProcessedBy 
            // ProcessedAt 
        };

        var created = await _productUpdateRequestRepository.CreateProductUpdateRequestWithTransactionAsync
            (productSnapshot, images, request, cancellationToken);
        
        var responseDto = _mapper.Map<ProductUpdateRequestResponseDTO>
            (await _productUpdateRequestRepository.GetProductUpdateRequestAsync(created.Item2, cancellationToken));
        
        responseDto.ProductSnapshot.Images = _mapper.Map<List<MediaLinkItemDTO>>
            (await _productUpdateRequestRepository.GetAllImagesByProductSnapshotIdAsync(created.Item1.Id, cancellationToken));
        responseDto.Product.Images = _mapper.Map<List<MediaLinkItemDTO>>
            (await _productUpdateRequestRepository.GetAllImagesByProductIdAsync(product.Id, cancellationToken));
        return responseDto;
    }
}