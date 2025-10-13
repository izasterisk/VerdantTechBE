using BLL.DTO;
using BLL.DTO.ProductCertificate;
using BLL.Interfaces;
using DAL.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductCertificateController : BaseController
    {
        private readonly IProductCertificateService _productCertificateService;
        public ProductCertificateController(IProductCertificateService productCertificateService)
        {
            _productCertificateService = productCertificateService;
        }
        /// <summary>
        ///  Vendor tạo mới chứng nhận sản phẩm
        /// </summary>
        /// <param name="productCertificateCreateDTO"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [EndpointSummary("Tạo mới chứng nhận sản phẩm")]
        public async Task<ActionResult<APIResponse>> CreateProductCertificate([FromBody]ProductCertificateCreateDTO productCertificateCreateDTO, CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var result = await _productCertificateService.CreateProductCertificateAsync(productCertificateCreateDTO, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        /// <summary>
        /// Lấy danh sách chứng nhận của sản phẩm theo ProductId
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("get-by-product-id/{productId}")]
        [EndpointSummary("Lấy danh sách chứng nhận của sản phẩm theo ProductId")]
        public async Task<ActionResult<APIResponse>> GetProductCertificatesByProductId([FromRoute] ulong productId, CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var result = await _productCertificateService.GetAllProductCertificatesByProductIdAsync(productId, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        /// <summary>
        /// Lấy chứng nhận sản phẩm theo Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("get-by-id/{id}")]
        [EndpointSummary("Lấy chứng nhận sản phẩm theo Id")]
        public async Task<ActionResult<APIResponse>> GetProductCertificateById([FromRoute] ulong id, CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var result = await _productCertificateService.GetProductCertificateByIdAsync(id, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        /// <summary>
        /// Cập nhật chứng nhận sản phẩm
        /// </summary>
        /// <param name="productCertificateUpdateDTO"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("update/{id}")]
        [EndpointSummary("Cập nhật chứng nhận sản phẩm")]
        public async Task<ActionResult<APIResponse>> UpdateProductCertificate([FromBody] ProductCertificateUpdateDTO productCertificateUpdateDTO, CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;
            try
            {
                var result = await _productCertificateService.UpdateProductCertificateAsync(productCertificateUpdateDTO, GetCancellationToken());
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
