using BLL.DTO.Crop;
using BLL.IService;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CropController : ControllerBase
    {
        private readonly ICropService _service;

        public CropController(ICropService service)
        {
            _service = service;
        }


        [HttpGet]
        [EndpointSummary("Lấy danh sách crop (phân trang)")]
        [EndpointDescription("Trả về danh sách crop đang active, hỗ trợ phân trang bằng page và pageSize.")]
        //[ProducesResponseType(typeof(IEnumerable<CropResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(page, pageSize, ct);
            return Ok(result);
        }


        [HttpGet("{id}")]
        [EndpointSummary("Lấy chi tiết một crop")]
        [EndpointDescription("Trả về thông tin chi tiết của một crop theo Id.")]
        //[ProducesResponseType(typeof(CropResponseDTO), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(ulong id, CancellationToken ct = default)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }


        [HttpPost]
        [EndpointSummary("Tạo mới crop (1 hoặc nhiều)")]
        [EndpointDescription("Tạo một hoặc nhiều crop cho cùng một FarmProfile dựa trên danh sách Crops trong CropCreateDto.")]
        //[ProducesResponseType(typeof(IEnumerable<CropResponseDTO>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] CropCreateDTO dto,
            CancellationToken ct = default)
        {
            var result = await _service.CreateAsync(dto, ct);

            // Nếu chỉ tạo 1, có thể trả CreatedAtAction cho bản ghi đầu tiên
            var first = result.FirstOrDefault();
            if (first is null)
                return BadRequest("Không tạo được crop nào.");

            return CreatedAtAction(nameof(GetById), new { id = first.Id }, result);
        }

        [HttpPut("{id}")]
        [EndpointSummary("Cập nhật thông tin một crop")]
        [EndpointDescription("Cập nhật tên cây trồng, ngày gieo trồng và trạng thái IsActive.")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            ulong id,
            [FromBody] CropUpdateDTO dto,
            CancellationToken ct = default)
        {
            var ok = await _service.UpdateAsync(id, dto, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("soft/{id}")]
        [EndpointSummary("Xóa mềm một crop")]
        [EndpointDescription("Đặt IsActive = false cho crop tương ứng, không xóa vật lý khỏi database.")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(ulong id, CancellationToken ct = default)
        {
            var ok = await _service.SoftDeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }


        [HttpDelete("hard/{id}")]
        [EndpointSummary("Xóa cứng một crop")]
        [EndpointDescription("Xóa vĩnh viễn crop khỏi database, không thể khôi phục.")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HardDelete(ulong id, CancellationToken ct = default)
        {
            var ok = await _service.HardDeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

    }
}
