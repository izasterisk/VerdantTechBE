using AutoMapper;
using BLL.DTO;
using BLL.DTO.ProductReview;
using BLL.DTO.MediaLink;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly VerdantTechDbContext _db;
    private readonly IMapper _mapper;

    public ProductReviewService(
        IProductReviewRepository reviewRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        VerdantTechDbContext db,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _db = db;
        _mapper = mapper;
    }

    public async Task<ProductReviewResponseDTO> CreateProductReviewAsync(
        ulong customerId, ProductReviewCreateDTO dto, List<MediaLinkItemDTO>? images, CancellationToken ct = default)
    {
        // Kiểm tra đơn hàng tồn tại và thuộc về customer
        var order = await _orderRepository.GetOrderWithRelationsByIdAsync(dto.OrderId, ct)
                    ?? throw new KeyNotFoundException("Đơn hàng không tồn tại.");

        if (order.CustomerId != customerId)
            throw new UnauthorizedAccessException("Bạn không có quyền đánh giá đơn hàng này.");

        // Kiểm tra đơn hàng đã được thanh toán (Paid, Shipped, hoặc Delivered)
        if (order.Status != OrderStatus.Paid && 
            order.Status != OrderStatus.Shipped && 
            order.Status != OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Chỉ có thể đánh giá sản phẩm sau khi đơn hàng đã được thanh toán.");
        }

        // Kiểm tra sản phẩm có trong đơn hàng không
        var orderDetail = order.OrderDetails.FirstOrDefault(od => od.ProductId == dto.ProductId)
                          ?? throw new InvalidOperationException("Sản phẩm không có trong đơn hàng này.");

        // Kiểm tra đã có review cho sản phẩm này trong đơn hàng này chưa
        var existingReview = await _reviewRepository.GetProductReviewByProductOrderCustomerAsync(
            dto.ProductId, dto.OrderId, customerId, useNoTracking: true, ct);

        if (existingReview != null)
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này trong đơn hàng này rồi.");

        // Tạo review
        var review = _mapper.Map<ProductReview>(dto);
        review.CustomerId = customerId;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        // Map images to MediaLink
        List<MediaLink>? mediaLinks = null;
        if (images != null && images.Count > 0)
        {
            mediaLinks = images.Select(img => new MediaLink
            {
                OwnerType = MediaOwnerType.ProductReviews,
                ImageUrl = img.ImageUrl,
                ImagePublicId = img.ImagePublicId,
                Purpose = MediaPurpose.None,
                SortOrder = img.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
        }

        var createdReview = await _reviewRepository.CreateProductReviewWithTransactionAsync(review, mediaLinks, ct);

        // Cập nhật rating trung bình của sản phẩm
        await UpdateProductRatingAverageAsync(dto.ProductId, ct);

        var responseDto = _mapper.Map<ProductReviewResponseDTO>(createdReview);
        if (createdReview.Customer != null)
        {
            responseDto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(createdReview.Customer);
        }

        // Load images
        var loadedImages = await _reviewRepository.GetAllImagesByReviewIdAsync(createdReview.Id, ct);
        responseDto.Images = _mapper.Map<List<ProductReviewImageDTO>>(loadedImages);

        return responseDto;
    }

    public async Task<ProductReviewResponseDTO?> GetProductReviewByIdAsync(
        ulong id, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetProductReviewByIdAsync(id, useNoTracking: true, ct);
        if (review is null) return null;

        var dto = _mapper.Map<ProductReviewResponseDTO>(review);
        if (review.Customer != null)
        {
            dto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(review.Customer);
        }

        // Load images
        var images = await _reviewRepository.GetAllImagesByReviewIdAsync(id, ct);
        dto.Images = _mapper.Map<List<ProductReviewImageDTO>>(images);

        return dto;
    }

    public async Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByProductIdAsync(
        ulong productId, int page, int pageSize, CancellationToken ct = default)
    {
        var (reviews, total) = await _reviewRepository.GetProductReviewsByProductIdAsync(productId, page, pageSize, ct);
        var dtos = _mapper.Map<List<ProductReviewResponseDTO>>(reviews);

        // Map customer info and images
        foreach (var dto in dtos)
        {
            var review = reviews.FirstOrDefault(r => r.Id == dto.Id);
            if (review?.Customer != null)
            {
                dto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(review.Customer);
            }
            
            // Load images
            var images = await _reviewRepository.GetAllImagesByReviewIdAsync(dto.Id, ct);
            dto.Images = _mapper.Map<List<ProductReviewImageDTO>>(images);
        }

        return ToPaged(dtos, total, page, pageSize);
    }

    public async Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByOrderIdAsync(
        ulong orderId, int page, int pageSize, CancellationToken ct = default)
    {
        var (reviews, total) = await _reviewRepository.GetProductReviewsByOrderIdAsync(orderId, page, pageSize, ct);
        var dtos = _mapper.Map<List<ProductReviewResponseDTO>>(reviews);

        // Map customer info and images
        foreach (var dto in dtos)
        {
            var review = reviews.FirstOrDefault(r => r.Id == dto.Id);
            if (review?.Customer != null)
            {
                dto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(review.Customer);
            }
            
            // Load images
            var images = await _reviewRepository.GetAllImagesByReviewIdAsync(dto.Id, ct);
            dto.Images = _mapper.Map<List<ProductReviewImageDTO>>(images);
        }

        return ToPaged(dtos, total, page, pageSize);
    }

    public async Task<PagedResponse<ProductReviewResponseDTO>> GetProductReviewsByCustomerIdAsync(
        ulong customerId, int page, int pageSize, CancellationToken ct = default)
    {
        var (reviews, total) = await _reviewRepository.GetProductReviewsByCustomerIdAsync(customerId, page, pageSize, ct);
        var dtos = _mapper.Map<List<ProductReviewResponseDTO>>(reviews);

        // Map customer info and images
        foreach (var dto in dtos)
        {
            var review = reviews.FirstOrDefault(r => r.Id == dto.Id);
            if (review?.Customer != null)
            {
                dto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(review.Customer);
            }
            
            // Load images
            var images = await _reviewRepository.GetAllImagesByReviewIdAsync(dto.Id, ct);
            dto.Images = _mapper.Map<List<ProductReviewImageDTO>>(images);
        }

        return ToPaged(dtos, total, page, pageSize);
    }

    public async Task<ProductReviewResponseDTO> UpdateProductReviewAsync(
        ulong customerId, ulong reviewId, ProductReviewUpdateDTO dto, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetProductReviewByIdAsync(reviewId, useNoTracking: false, ct)
                     ?? throw new KeyNotFoundException("Đánh giá không tồn tại.");

        if (review.CustomerId != customerId)
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật đánh giá này.");

        // Cập nhật các field
        if (dto.Rating.HasValue)
            review.Rating = dto.Rating.Value;

        if (dto.Comment != null)
            review.Comment = dto.Comment;

        review.UpdatedAt = DateTime.UtcNow;

        var updatedReview = await _reviewRepository.UpdateProductReviewAsync(review, ct);

        // Cập nhật rating trung bình của sản phẩm
        await UpdateProductRatingAverageAsync(review.ProductId, ct);

        var responseDto = _mapper.Map<ProductReviewResponseDTO>(updatedReview);
        if (updatedReview.Customer != null)
        {
            responseDto.Customer = _mapper.Map<BLL.DTO.User.UserResponseDTO>(updatedReview.Customer);
        }

        // Load images
        var images = await _reviewRepository.GetAllImagesByReviewIdAsync(updatedReview.Id, ct);
        responseDto.Images = _mapper.Map<List<ProductReviewImageDTO>>(images);

        return responseDto;
    }

    public async Task<bool> DeleteProductReviewAsync(
        ulong customerId, ulong reviewId, CancellationToken ct = default)
    {
        var review = await _reviewRepository.GetProductReviewByIdAsync(reviewId, useNoTracking: false, ct)
                     ?? throw new KeyNotFoundException("Đánh giá không tồn tại.");

        if (review.CustomerId != customerId)
            throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này.");

        var productId = review.ProductId;
        
        // Xóa ảnh liên quan trước
        var images = await _reviewRepository.GetAllImagesByReviewIdAsync(reviewId, ct);
        if (images.Count > 0)
        {
            _db.MediaLinks.RemoveRange(images);
            await _db.SaveChangesAsync(ct);
        }
        
        var deleted = await _reviewRepository.DeleteProductReviewAsync(reviewId, ct);

        if (deleted)
        {
            // Cập nhật rating trung bình của sản phẩm
            await UpdateProductRatingAverageAsync(productId, ct);
        }

        return deleted;
    }

    private async Task UpdateProductRatingAverageAsync(ulong productId, CancellationToken ct)
    {
        var average = await _reviewRepository.CalculateProductRatingAverageAsync(productId, ct);
        
        var product = await _productRepository.GetProductByIdAsync(productId, useNoTracking: false, ct);
        if (product != null)
        {
            product.RatingAverage = average;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateProductAsync(product, ct);
        }
    }

    private static PagedResponse<T> ToPaged<T>(List<T> items, int totalRecords, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        return new PagedResponse<T>
        {
            Data = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}

