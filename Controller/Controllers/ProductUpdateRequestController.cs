using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.ProductUpdateRequest;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Cloudinary;

namespace Controller.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductUpdateRequestController : BaseController
{
    private readonly IProductUpdateRequestService _service;
    private readonly ICloudinaryService _cloudinary;
    
    public ProductUpdateRequestController(
        IProductUpdateRequestService service,
        ICloudinaryService cloudinary)
    {
        _service = service;
        _cloudinary = cloudinary;
    }

    /// <summary>
    /// Tạo yêu cầu cập nhật sản phẩm mới (với ảnh)
    /// </summary>
    /// <param name="form">Thông tin yêu cầu cập nhật và file ảnh</param>
    /// <returns>Thông tin yêu cầu đã tạo</returns>
    [HttpPost("create-with-images")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Vendor")]
    [EndpointSummary("Create Product Update Request with Images")]
    [EndpointDescription("Tạo yêu cầu cập nhật sản phẩm kèm theo upload ảnh mới. " +
                         "Gửi thông tin DTO qua form-data với prefix 'Data.' và file ảnh qua 'MainImage' và 'Gallery'. " +
                         "Ví dụ: Data.ProductId=123, Data.NewName='Sản phẩm mới', MainImage=file, Gallery=file1, Gallery=file2")]
    public async Task<ActionResult<APIResponse>> CreateWithImages(
        [FromForm] CreateProductUpdateRequestForm form)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            // TODO: Upload images to Cloudinary
            string? mainImageUrl = null;
            if (form.MainImage != null)
            {
                var uploadResult = await _cloudinary.UploadAsync(
                    form.MainImage, 
                    "product-updates/main", 
                    GetCancellationToken()
                );
                mainImageUrl = uploadResult.Url;
            }

            var galleryUrls = new List<string>();
            if (form.Gallery?.Count > 0)
            {
                var uploads = await _cloudinary.UploadManyAsync(
                    form.Gallery, 
                    "product-updates/gallery", 
                    GetCancellationToken()
                );
                galleryUrls = uploads.Select(x => x.Url).ToList();
            }

            // TODO: Call service to create request
            // var result = await _service.CreateAsync(form.Data, mainImageUrl, galleryUrls, GetCancellationToken());

            // Mock response for demonstration
            var mockResponse = new
            {
                Id = 1,
                ProductId = form.Data.ProductId,
                NewName = form.Data.NewName,
                NewPrice = form.Data.NewPrice,
                MainImageUrl = mainImageUrl,
                GalleryUrls = galleryUrls,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return SuccessResponse(mockResponse, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Tạo yêu cầu cập nhật sản phẩm (chỉ JSON)
    /// </summary>
    /// <param name="dto">Thông tin yêu cầu cập nhật</param>
    /// <returns>Thông tin yêu cầu đã tạo</returns>
    [HttpPost("create")]
    [Authorize(Roles = "Vendor")]
    [EndpointSummary("Create Product Update Request (JSON only)")]
    [EndpointDescription("Tạo yêu cầu cập nhật sản phẩm chỉ với dữ liệu JSON, không upload ảnh.")]
    public async Task<ActionResult<APIResponse>> Create(
        [FromBody] ProductUpdateRequestCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            // TODO: Call service
            // var result = await _service.CreateAsync(dto, GetCancellationToken());

            // Mock response
            var mockResponse = new
            {
                Id = 1,
                ProductId = dto.ProductId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return SuccessResponse(mockResponse, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách yêu cầu cập nhật sản phẩm
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <param name="status">Trạng thái để filter (Pending, Approved, Rejected)</param>
    /// <returns>Danh sách yêu cầu có phân trang</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Vendor")]
    [EndpointSummary("Get All Product Update Requests")]
    [EndpointDescription("Lấy danh sách tất cả yêu cầu cập nhật sản phẩm với phân trang và filter theo status. " +
                         "Ví dụ: /api/ProductUpdateRequest?page=1&pageSize=20&status=Pending")]
    public async Task<ActionResult<APIResponse>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? status = null)
    {
        try
        {
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            // TODO: Call service
            // var result = await _service.GetAllAsync(page, pageSize, status, GetCancellationToken());

            // Mock response
            var mockResponse = new
            {
                TotalCount = 50,
                Page = page,
                PageSize = pageSize,
                Items = new[]
                {
                    new { Id = 1, ProductId = 123, Status = "Pending" },
                    new { Id = 2, ProductId = 456, Status = "Approved" }
                }
            };

            return SuccessResponse(mockResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy chi tiết yêu cầu cập nhật sản phẩm
    /// </summary>
    /// <param name="id">ID của yêu cầu</param>
    /// <returns>Thông tin chi tiết yêu cầu</returns>
    [HttpGet("{id}")]
    [Authorize]
    [EndpointSummary("Get Product Update Request By ID")]
    [EndpointDescription("Lấy thông tin chi tiết của một yêu cầu cập nhật sản phẩm theo ID.")]
    public async Task<ActionResult<APIResponse>> GetById(ulong id)
    {
        try
        {
            // TODO: Call service
            // var result = await _service.GetByIdAsync(id, GetCancellationToken());

            // Mock response
            var mockResponse = new
            {
                Id = id,
                ProductId = 123,
                NewName = "Sản phẩm cập nhật",
                NewPrice = 150000,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return SuccessResponse(mockResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật trạng thái yêu cầu (Approve/Reject)
    /// </summary>
    /// <param name="id">ID của yêu cầu</param>
    /// <param name="dto">Thông tin cập nhật trạng thái</param>
    /// <returns>Thông tin yêu cầu đã cập nhật</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Update Request Status")]
    [EndpointDescription("Cập nhật trạng thái yêu cầu (Approved/Rejected). Chỉ Admin/Staff mới có quyền.")]
    public async Task<ActionResult<APIResponse>> UpdateStatus(
        ulong id, 
        [FromBody] UpdateStatusDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            // TODO: Call service
            // var result = await _service.UpdateStatusAsync(id, dto, GetCancellationToken());

            // Mock response
            var mockResponse = new
            {
                Id = id,
                Status = dto.Status,
                ReviewNote = dto.ReviewNote,
                UpdatedAt = DateTime.UtcNow
            };

            return SuccessResponse(mockResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    #region Form Classes
    
    /// <summary>
    /// Form để upload sản phẩm cập nhật kèm ảnh
    /// </summary>
    public sealed class CreateProductUpdateRequestForm
    {
        [FromForm] public ProductUpdateRequestCreateDTO Data { get; set; } = null!;
        [FromForm] public IFormFile? MainImage { get; set; }
        [FromForm] public List<IFormFile>? Gallery { get; set; }
    }

    #endregion
}
