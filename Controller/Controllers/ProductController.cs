using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BLL.DTO;
using BLL.DTO.Product;
using BLL.DTO.MediaLink;
using BLL.Interfaces;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _svc;

        public ProductController(IProductService svc)
        {
            _svc = svc;
        }

        // ========= READS =========

        [HttpGet]
        [EndpointSummary("Danh sách sản phẩm (phân trang)")]
        [EndpointDescription("Trả về danh sách sản phẩm kèm thông tin cơ bản.")]
        public async Task<ActionResult<PagedResponse<ProductListItemDTO>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => Ok(await _svc.GetAllAsync(page, pageSize, ct));

        [HttpGet("{id:long}")]
        [EndpointSummary("Lấy chi tiết sản phẩm theo Id")]
        [EndpointDescription("Bao gồm đầy đủ thông tin và toàn bộ ảnh (MediaLink) của sản phẩm.")]
        public async Task<ActionResult<ProductResponseDTO>> GetById(
            long id,
            CancellationToken ct = default)
        {
            var item = await _svc.GetByIdAsync((ulong)id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("category/{categoryId:long}")]
        [EndpointSummary("Danh sách sản phẩm theo Category (phân trang)")]
        [EndpointDescription("Lọc theo CategoryId, trả về danh sách kèm thông tin cơ bản.")]
        public async Task<ActionResult<PagedResponse<ProductListItemDTO>>> GetByCategory(
            long categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => Ok(await _svc.GetByCategoryAsync((ulong)categoryId, page, pageSize, ct));

        [HttpGet("vendor/{vendorId:long}")]
        [EndpointSummary("Danh sách sản phẩm theo Vendor (phân trang)")]
        [EndpointDescription("Lọc theo VendorId, trả về danh sách kèm thông tin cơ bản.")]
        public async Task<ActionResult<PagedResponse<ProductListItemDTO>>> GetByVendor(
            long vendorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
            => Ok(await _svc.GetByVendorAsync((ulong)vendorId, page, pageSize, ct));

        // ========= UPDATE =========

        [HttpPut("{id:long}")]
        [EndpointSummary("Cập nhật sản phẩm + ảnh (add/remove)")]
        [EndpointDescription(@"Body JSON:
{
  ""data"": ProductUpdateDTO (các field cơ bản, KHÔNG cần Id),
  ""addImages"": [ { ImageUrl, ImagePublicId, Purpose, SortOrder }, ... ],
  ""removeImagePublicIds"": [ ""publicId1"", ""publicId2"" ]
}
Ảnh được quản lý trong MediaLink với OwnerType = Product.")]
        public async Task<ActionResult<ProductResponseDTO>> Update(
            long id,
            [FromBody] UpdateRequest body,
            CancellationToken ct = default)
        {
            if (body?.Data is null) return BadRequest("Thiếu Data.");

            // Dùng Id từ route — không cần Id trong body
            body.Data.Id = (ulong)id;

            var add = body.AddImages ?? new List<MediaLinkItemDTO>();
            var removed = body.RemoveImagePublicIds ?? new List<string>();

            var updated = await _svc.UpdateAsync((ulong)id, body.Data, add, removed, ct);
            return Ok(updated);
        }

        // ========= UPDATE EMISSION (CommissionRate) =========

        [HttpPatch("{id}/emission")]
        [EndpointSummary("Cập nhật CommissionRate của sản phẩm")]
        [EndpointDescription(@"Body JSON:
{ ""commissionRate"": 0.05 }   // ví dụ 5%
Route chứa id, server sẽ set ProductId từ route.")]
        public async Task<IActionResult> UpdateEmission(
            long id,
            [FromBody] ProductUpdateEmissionDTO dto,
            CancellationToken ct = default)
        {
            if (dto is null) return BadRequest("Thiếu dữ liệu.");
            dto.Id = (ulong)id;

            var ok = await _svc.UpdateEmissionAsync(dto, ct);
            return ok ? NoContent() : NotFound();
        }

        // ========= DELETE =========

        [HttpDelete("{id:long}")]
        [EndpointSummary("Xoá sản phẩm")]
        [EndpointDescription("Xoá sản phẩm và (tuỳ bạn xử lý ở repo/service) có thể dọn ảnh MediaLink liên quan.")]
        public async Task<IActionResult> Delete(
            long id,
            CancellationToken ct = default)
            => (await _svc.DeleteAsync((ulong)id, ct)) ? NoContent() : NotFound();

        // ========= Request models =========

        public sealed class UpdateRequest
        {
            public ProductUpdateDTO Data { get; set; } = null!;
            public List<MediaLinkItemDTO>? AddImages { get; set; }
            public List<string>? RemoveImagePublicIds { get; set; }
        }
    }
}
