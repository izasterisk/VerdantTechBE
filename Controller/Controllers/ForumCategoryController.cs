using BLL.DTO.ForumCategory;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ForumCategoryController : ControllerBase
{
    private readonly IForumCategoryService _service;

    public ForumCategoryController(IForumCategoryService service)
    {
        _service = service;
    }

    // ================== GET ALL ==================
    [HttpGet]
    [EndpointSummary("Lấy danh sách Forum Category")]
    [EndpointDescription("Lấy toàn bộ danh mục theo trang (page, pageSize).")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var data = await _service.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(data);
    }

    // ================== GET BY ID ==================
    [HttpGet("{id}")]
    [EndpointSummary("Lấy chi tiết 1 Forum Category")]
    [EndpointDescription("Trả về chi tiết của một danh mục theo Id.")]
    public async Task<IActionResult> GetById(
        ulong id,
        CancellationToken cancellationToken = default)
    {
        var data = await _service.GetByIdAsync(id, cancellationToken);
        if (data is null)
            return NotFound();

        return Ok(data);
    }

    // ================== CREATE ==================
    [HttpPost]
    [EndpointSummary("Tạo mới Forum Category")]
    [EndpointDescription("Tạo mới danh mục forum dựa trên dữ liệu gửi lên.")]
    public async Task<IActionResult> Create(
        [FromBody] ForumCategoryCreateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await _service.CreateAsync(dto, cancellationToken);
        return Ok(new { message = "Created successfully" });
    }

    // ================== UPDATE ==================
    [HttpPut("{id}")]
    [EndpointSummary("Cập nhật Forum Category")]
    [EndpointDescription("Cập nhật danh mục dựa trên Id và dữ liệu truyền vào.")]
    public async Task<IActionResult> Update(
        ulong id,
        [FromBody] ForumCategoryUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdateAsync(id, dto, cancellationToken);
        return Ok(new { message = "Updated successfully" });
    }

    // ================== DELETE ==================
    [HttpDelete("{id}")]
    [EndpointSummary("Xóa Forum Category")]
    [EndpointDescription("Xóa một danh mục theo Id.")]
    public async Task<IActionResult> Delete(
        ulong id,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(new { message = "Deleted successfully" });
    }
}
