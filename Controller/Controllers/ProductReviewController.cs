using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.DTO;
using BLL.DTO.ProductReview;
using BLL.DTO.MediaLink;
using BLL.Interfaces;
using Infrastructure.Cloudinary;
using BLL.Interfaces.Infrastructure;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductReviewController : BaseController
{
    private readonly IProductReviewService _reviewService;
    private readonly ICloudinaryService _cloudinary;

    public ProductReviewController(IProductReviewService reviewService, ICloudinaryService cloudinary)
    {
        _reviewService = reviewService;
        _cloudinary = cloudinary;
    }

    /// <summary>
    /// Tạo đánh giá sản phẩm
    /// </summary>
    /// <param name="form">Form data với thông tin đánh giá và ảnh</param>
    /// <returns>Thông tin đánh giá đã tạo</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Create Product Review")]
    [EndpointDescription("Tạo đánh giá sản phẩm sau khi đơn hàng đã được thanh toán. " +
                         "Chỉ có thể đánh giá một lần cho mỗi sản phẩm trong mỗi đơn hàng. " +
                         "Có thể upload nhiều ảnh kèm theo.")]
    public async Task<ActionResult<APIResponse>> CreateProductReview([FromForm] CreateReviewForm form)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var customerId = GetCurrentUserId();
            
            // Upload ảnh lên Cloudinary nếu có
            List<MediaLinkItemDTO>? imagesDto = null;
            if (form.Images != null && form.Images.Count > 0)
            {
                var uploadResults = await _cloudinary.UploadManyAsync(form.Images, "product-reviews/images", GetCancellationToken());
                imagesDto = uploadResults.Select((x, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = x.PublicId,
                    ImageUrl = x.Url,
                    Purpose = "none",
                    SortOrder = i + 1
                }).ToList();
            }

            var review = await _reviewService.CreateProductReviewAsync(customerId, form.Data, imagesDto, GetCancellationToken());
            return SuccessResponse(review, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin đánh giá theo ID
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Thông tin đánh giá</returns>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get Product Review By ID")]
    [EndpointDescription("Lấy thông tin chi tiết một đánh giá sản phẩm theo ID.")]
    public async Task<ActionResult<APIResponse>> GetProductReviewById(long id)
    {
        try
        {
            var review = await _reviewService.GetProductReviewByIdAsync((ulong)id, GetCancellationToken());
            if (review == null)
                return ErrorResponse("Đánh giá không tồn tại.", HttpStatusCode.NotFound);
            
            return SuccessResponse(review);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá theo Product ID
    /// </summary>
    /// <param name="productId">ID của sản phẩm</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 20)</param>
    /// <returns>Danh sách đánh giá có phân trang</returns>
    [HttpGet("product/{productId:long}")]
    [EndpointSummary("Get Product Reviews By Product ID")]
    [EndpointDescription("Lấy danh sách đánh giá của một sản phẩm với phân trang.")]
    public async Task<ActionResult<APIResponse>> GetProductReviewsByProductId(
        long productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");
            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var reviews = await _reviewService.GetProductReviewsByProductIdAsync(
                (ulong)productId, page, pageSize, GetCancellationToken());
            return SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá theo Order ID
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 20)</param>
    /// <returns>Danh sách đánh giá có phân trang</returns>
    [HttpGet("order/{orderId:long}")]
    [Authorize]
    [EndpointSummary("Get Product Reviews By Order ID")]
    [EndpointDescription("Lấy danh sách đánh giá của một đơn hàng với phân trang.")]
    public async Task<ActionResult<APIResponse>> GetProductReviewsByOrderId(
        long orderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");
            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var reviews = await _reviewService.GetProductReviewsByOrderIdAsync(
                (ulong)orderId, page, pageSize, GetCancellationToken());
            return SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá theo Customer ID
    /// </summary>
    /// <param name="customerId">ID của khách hàng</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 20)</param>
    /// <returns>Danh sách đánh giá có phân trang</returns>
    [HttpGet("customer/{customerId:long}")]
    [Authorize]
    [EndpointSummary("Get Product Reviews By Customer ID")]
    [EndpointDescription("Lấy danh sách đánh giá của một khách hàng với phân trang.")]
    public async Task<ActionResult<APIResponse>> GetProductReviewsByCustomerId(
        long customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");
            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var reviews = await _reviewService.GetProductReviewsByCustomerIdAsync(
                (ulong)customerId, page, pageSize, GetCancellationToken());
            return SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật đánh giá sản phẩm
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <param name="dto">Thông tin cập nhật</param>
    /// <returns>Thông tin đánh giá đã cập nhật</returns>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Update Product Review")]
    [EndpointDescription("Cập nhật đánh giá sản phẩm. Chỉ chủ sở hữu đánh giá mới có quyền cập nhật.")]
    public async Task<ActionResult<APIResponse>> UpdateProductReview(
        long id,
        [FromBody] ProductReviewUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var customerId = GetCurrentUserId();
            var review = await _reviewService.UpdateProductReviewAsync(
                customerId, (ulong)id, dto, GetCancellationToken());
            return SuccessResponse(review);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xóa đánh giá sản phẩm
    /// </summary>
    /// <param name="id">ID của đánh giá</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Delete Product Review")]
    [EndpointDescription("Xóa đánh giá sản phẩm. Chỉ chủ sở hữu đánh giá mới có quyền xóa.")]
    public async Task<ActionResult<APIResponse>> DeleteProductReview(long id)
    {
        try
        {
            var customerId = GetCurrentUserId();
            var deleted = await _reviewService.DeleteProductReviewAsync(
                customerId, (ulong)id, GetCancellationToken());
            
            if (deleted)
                return SuccessResponse(null, HttpStatusCode.NoContent);
            else
                return ErrorResponse("Không thể xóa đánh giá.", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // ========= Request models =========

    public sealed class CreateReviewForm
    {
        [FromForm] public ProductReviewCreateDTO Data { get; set; } = null!;
        [FromForm] public List<IFormFile>? Images { get; set; }
    }
}

