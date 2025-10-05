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
        public async Task<IActionResult> RegisterProduct([FromBody] ProductRegistrationCreateDTO requestDTO, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var vendorid = GetCurrentUserId();
            var result = await _productService.ProductRegistrationAsync(vendorid, requestDTO, cancellationToken);
            return Ok(result);
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
