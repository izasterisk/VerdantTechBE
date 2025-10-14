using BLL.DTO;
using BLL.DTO.Product;
using BLL.DTO.ProductRegistration;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        /// <summary>
        /// Đăng ký sản phẩm mới từ nhà cung cấp, đợi phê duyệt từ staff trước khi hiển thị trên nền tảng
        /// </summary>
        /// <param name="requestDTO"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("register-product")]
        [EndpointSummary("Register Product By VendorID")]
        [EndpointDescription("đăng ký sản phẩm của vendor và đợi staff duyệt .")]
        public async Task<ActionResult<APIResponse>> RegisterProduct([FromBody] ProductRegistrationCreateDTO requestDTO)
        {
            try
            {
                var vendorid = GetCurrentUserId();
                var result = await _productService.ProductRegistrationAsync(vendorid, requestDTO, GetCancellationToken());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);

            }
        }

        /// <summary>
        /// Get toàn bộ sản phẩm đã đăng ký của nhà cung cấp (vendor) hiện tại
        /// </summary>
        /// <returns></returns>
        [HttpGet("product-registrations")]
        [EndpointSummary("Get All Product By VendorID")]
        [EndpointDescription("Lấy toàn bộ thông tin sản phẩm đã đăng ký theo VendorID.")]
        public async Task<ActionResult<APIResponse>> GetAllProductRegistrationsByVendorId()
        {
            try
            {
                var vendorid = GetCurrentUserId();
                var result = await _productService.GetAllProductByVendorIdAsync(vendorid, GetCancellationToken());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

        }
            /// <summary>
            /// Lấy thông tin sản phẩm theo ID
            /// </summary>
            /// <param name="id">ID của sản phẩm</param>
            /// <returns>Thông tin sản phẩm</returns>
            [HttpGet("{id}")]
        [EndpointSummary("Get Product By ID")]
        [EndpointDescription("Lấy thông tin  sản phẩm theo ID.")]
        public async Task<ActionResult<APIResponse>> GetProductById([FromRoute] ulong id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id, GetCancellationToken());

                if (result == null)
                    return ErrorResponse($"Không tìm thấy sản phẩm với ID {id}", HttpStatusCode.NotFound);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        /// <returns>Danh sách sản phẩm</returns>
        [HttpGet]
        [EndpointSummary("Get All Product")]
        [EndpointDescription("Lấy danh sách tất cả sản phẩm.")]
        public async Task<ActionResult<APIResponse>> GetAllProduct()
        {
            try
            {
                var list = await _productService.GetAllProductAsync(GetCancellationToken());
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        /// <summary>
        /// Lấy danh sách tất cả sản phẩm theo category ID
        /// </summary>
        /// <returns>Danh sách sản phẩm</returns>
        [HttpGet("category/{id}")]
        [EndpointSummary("Get All Product By Category")]
        [EndpointDescription("Lấy danh sách tất cả sản phẩm theo category ID.")]
        public async Task<ActionResult<APIResponse>> GetAllProductByCategory([FromRoute] ulong id)
        {
            try
            {
                var list = await _productService.GetAllProductByCategoryIdAsync(id,GetCancellationToken());
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        /// <summary>
        /// Lấy danh sách tất cả product registration với phân trang 
        /// </summary>
        /// <param name="page">Số trang (mặc định: 1)</param>
        /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
        /// <returns>Danh sách product registration có phân trang</returns>
        [HttpGet("registrations")]
        [Authorize(Roles = "Admin,Staff")]
        [EndpointSummary("Get All Product Registration (note.)")]
        public async Task<ActionResult<APIResponse>> GetAllProductRegistraion([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1)
                    return ErrorResponse("Page number must be greater than 0");

                if (pageSize < 1 || pageSize > 100)
                    return ErrorResponse("Page size must be between 1 and 100");

                var users = await _productService.GetAllProductRegisterAsync(page, pageSize, GetCancellationToken());
                return SuccessResponse(users);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <param name="dto">Thông tin sản phẩm cần cập nhật</param>
        /// <returns>Thông tin sản phẩm đã cập nhật</returns>
        [HttpPut("{id}")]
        [EndpointSummary("Update Product ")]
        [EndpointDescription("Cập nhật thông tin sản phẩm.")]
        public async Task<ActionResult<APIResponse>> UpdateProduct([FromRoute] ulong id, [FromBody] ProductUpdateDTO dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                var result = await _productService.UpdateProductAsync(id, dto, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return ErrorResponse("Không tìm thấy sản phẩm", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
