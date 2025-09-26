using BLL.DTO;
using BLL.DTO.ProductCategory;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductCategoryController : BaseController
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        /// <summary>
        /// Tạo danh mục sản phẩm mới cho người dùng hiện tại
        /// </summary>
        /// <param name="dto">Thông tin danh mục sản phẩm cần tạo</param>
        /// <returns>Thông tin danh mục sản phẩm đã tạo</returns>
        [HttpPost]
        [EndpointSummary("Create Product Category")]
        public async Task<ActionResult<APIResponse>> CreateProductCategory([FromBody] ProductCategoryCreateDTO dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                var result = await _productCategoryService.CreateProductCategoryAsync(dto, GetCancellationToken());
                return SuccessResponse(result, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Lấy thông tin danh mục sản phẩm theo ID
        /// </summary>
        /// <param name="id">ID của danh mục sản phẩm</param>
        /// <returns>Thông tin danh mục sản phẩm</returns>
        [HttpGet("{id}")]
        [EndpointSummary("Get Product Category By ID")]
        public async Task<ActionResult<APIResponse>> GetProductCategoryById([FromRoute] ulong id)
        {
            try
            {
                var result = await _productCategoryService.GetProductCategoryByIdAsync(id, GetCancellationToken());

                if (result == null)
                    return ErrorResponse($"Không tìm thấy danh mục sản phẩm với ID {id}", HttpStatusCode.NotFound);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục sản phẩm
        /// </summary>
        /// <returns>Danh sách danh mục sản phẩm</returns>
        [HttpGet]
        [EndpointSummary("Get All Product Categories")]
        public async Task<ActionResult<APIResponse>> GetAllProductCategories()
        {
            try
            {
                var list = await _productCategoryService.GetAllProductCategoryAsync(GetCancellationToken());
                return SuccessResponse(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Cập nhật thông tin danh mục sản phẩm
        /// </summary>
        /// <param name="id">ID của danh mục sản phẩm</param>
        /// <param name="dto">Thông tin danh mục sản phẩm cần cập nhật</param>
        /// <returns>Thông tin danh mục sản phẩm đã cập nhật</returns>
        [HttpPut("{id}")]
        [EndpointSummary("Update Product Category")]
        [EndpointDescription("Nếu đã là Category cha (tức có một category khác có ParentId là Id của cái này thì không thể làm category con của 1 cái khác.")]
        public async Task<ActionResult<APIResponse>> UpdateProductCategory([FromRoute] ulong id, [FromBody] ProductCategoryUpdateDTO dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            try
            {
                var result = await _productCategoryService.UpdateProductCategoryAsync(id, dto, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return ErrorResponse("Không tìm thấy danh mục sản phẩm", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
