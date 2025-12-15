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
        
        var productImages = await _productUpdateRequestRepository.GetAllImagesByProductIdAsync(product.Id, cancellationToken);
        var imagesToKeep = productImages;
        var nextSortOrder = 1;
        if(dto.ImagesToDelete != null && dto.ImagesToDelete.Count > 0)
        {
            var imagesToKeepDict = imagesToKeep.ToDictionary(img => img.Id);
            foreach (var imageId in dto.ImagesToDelete)
            {
                if (!imagesToKeepDict.Remove(imageId))
                    throw new InvalidOperationException($"Hình ảnh với ID {imageId} không tồn tại hoặc không thuộc về sản phẩm này.");
            }
            imagesToKeep = imagesToKeepDict
                .Values
                .OrderBy(img => img.SortOrder)
                .ToList();
        }
        foreach (var imageToKeep in imagesToKeep)
        {
            imageToKeep.Id = 0;
            imageToKeep.OwnerType = MediaOwnerType.ProductSnapshot;
            imageToKeep.SortOrder = nextSortOrder;
            nextSortOrder++;
        }
        
        var imagesToAdd = new List<MediaLink>();
        if(dto.ImagesToAdd != null && dto.ImagesToAdd.Count > 0)
        {
            var uploadedImages = await Utils.UploadImagesAsync(
                _cloudinaryService, dto.ImagesToAdd,
                "product-update-requests/images", "productImage", 
                nextSortOrder, cancellationToken
            );
            foreach (var img in uploadedImages)
            {
                imagesToAdd.Add(new MediaLink
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
            (productSnapshot, imagesToAdd, imagesToKeep, request, cancellationToken);
        
        var responseDto = _mapper.Map<ProductUpdateRequestResponseDTO>
            (await _productUpdateRequestRepository.GetProductUpdateRequestWithRelationsByIdAsync(created.Item2, cancellationToken));
        
        responseDto.ProductSnapshot.Images = _mapper.Map<List<MediaLinkItemDTO>>
            (await _productUpdateRequestRepository.GetAllImagesByProductSnapshotIdAsync(created.Item1.Id, cancellationToken));
        responseDto.Product.Images = _mapper.Map<List<MediaLinkItemDTO>>
            (productImages);
        return responseDto;
    }
    
    public async Task<ProductUpdateRequestResponseDTO> ProcessProductUpdateRequestAsync(ulong staffId, ulong requestId, ProductUpdateRequestUpdateDTO dto, CancellationToken cancellationToken)
    {
        if(dto.Status == ProductRegistrationStatus.Pending)
            throw new InvalidOperationException("Trạng thái yêu cầu cập nhật sản phẩm không thể là 'Đang chờ xử lý' khi xử lý.");
        var request = await _productUpdateRequestRepository.GetProductUpdateRequestWithRelationsByIdAsync(requestId, cancellationToken);
        if(request.Status != ProductRegistrationStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể xử lý các yêu cầu đang chờ xử lý.");
        
        request.Status = dto.Status;
        request.ProcessedBy = staffId;
        request.ProcessedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        if (dto.Status == ProductRegistrationStatus.Approved)
        {
            if(dto.RejectionReason != null)
                throw new InvalidOperationException("Chỉ có thể cung cấp lý do từ chối khi trạng thái là 'Từ chối'.");
            
            var productSnapshot = _mapper.Map<ProductSnapshot>(request.Product);
            productSnapshot.SnapshotType = ProductSnapshotType.History;
            var productSnapshotImages = await _productUpdateRequestRepository.GetAllImagesByProductIdAsync
                (request.Product.Id, cancellationToken);

            var productUpdate = _mapper.Map<Product>(request.ProductSnapshot);
            productUpdate.Id = request.ProductSnapshot.ProductId;
            var productUpdateImages = await _productUpdateRequestRepository.GetAllImagesByProductSnapshotIdAsync
                (request.ProductSnapshot.Id, cancellationToken);
            
            request.Product = null!;
            request.ProductSnapshot = null!;
            await _productUpdateRequestRepository.ApproveProductUpdateRequestAsync
                (request, productSnapshot, productSnapshotImages, productUpdate, productUpdateImages, cancellationToken);
        }
        else
        {
            request.RejectionReason = dto.RejectionReason;
            request.Product = null!;
            request.ProductSnapshot = null!;
            await _productUpdateRequestRepository.RejectProductUpdateRequestAsync(request, cancellationToken);
        }
        
        var response = _mapper.Map<ProductUpdateRequestResponseDTO>
            (await _productUpdateRequestRepository.GetProductUpdateRequestByIdAsync(requestId, cancellationToken));
        if(response.Status == ProductRegistrationStatus.Approved)
            response.Product = _mapper.Map<FullyProductResponseDTO>
                (await _productUpdateRequestRepository.GetProductByIdAsync(request.ProductId, cancellationToken));
        return response;
    }
    
    public async Task<(List<ProductUpdateRequestResponseDTO>, int totalCount)> GetAllProductUpdateRequestsAsync
        (int page, int pageSize, ProductRegistrationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var (requests, totalCount) = await _productUpdateRequestRepository.GetAllProductUpdateRequestsAsync(page, pageSize, status, cancellationToken);
        
        var responseDtos = new List<ProductUpdateRequestResponseDTO>();
        
        foreach (var request in requests)
        {
            var responseDto = _mapper.Map<ProductUpdateRequestResponseDTO>(request);
            
            responseDto.ProductSnapshot.Images = _mapper.Map<List<MediaLinkItemDTO>>
                (await _productUpdateRequestRepository.GetAllImagesByProductSnapshotIdAsync(request.ProductSnapshot.Id, cancellationToken));
            responseDto.Product.Images = _mapper.Map<List<MediaLinkItemDTO>>
                (await _productUpdateRequestRepository.GetAllImagesByProductIdAsync(request.Product.Id, cancellationToken));
            
            responseDtos.Add(responseDto);
        }
        return (responseDtos, totalCount);
    }
}