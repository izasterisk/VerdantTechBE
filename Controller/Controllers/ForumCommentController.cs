using BLL.DTO.ForumComment;
using BLL.Interfaces;
using DAL.Data;
using DAL.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumCommentController : ControllerBase
    {
        private readonly IForumCommentService _service;

        public ForumCommentController(IForumCommentService service)
        {
            _service = service;
        }

        // GET COMMENTS BY POST
        [HttpGet("post/{postId}")]
        [EndpointSummary("Lấy danh sách comment cha của bài viết")]
        [EndpointDescription("Lấy bình luận cha (parentId=null) theo phân trang và bao gồm replies (nested) bên trong.")]
        public async Task<IActionResult> GetByPost(
            ulong postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _service.GetCommentsByPostIdAsync(postId, page, pageSize, ct);
            return Ok(result);
        }

        // DETAIL
        [HttpGet("{id}")]
        [EndpointSummary("Lấy chi tiết 1 comment")]
        [EndpointDescription("Bao gồm toàn bộ replies dạng tree.")]
        public async Task<IActionResult> Detail(ulong id, CancellationToken ct = default)
        {
            var result = await _service.GetDetailAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // CREATE
        [HttpPost]
        [EndpointSummary("Tạo comment mới / Reply comment")]
        [EndpointDescription("Tự động gửi notification đến chủ bài viết hoặc chủ comment cha.")]
        public async Task<IActionResult> Create(
            [FromBody] ForumCommentCreateDTO dto,
            CancellationToken ct = default)
        {
            var result = await _service.CreateAsync(dto, ct);
            return Ok(result);
        }

        // UPDATE
        [HttpPut("{id}")]
        [EndpointSummary("Cập nhật nội dung comment")]
        [EndpointDescription("Người cập nhật phải trùng UserId trong comment.")]
        public async Task<IActionResult> Update(
            ulong id,
            [FromBody] ForumCommentUpdateDTO dto,
            CancellationToken ct = default)
        {
            if (id != dto.Id)
                return BadRequest("Id trong route không trùng với Id trong body.");

            var result = await _service.UpdateAsync(dto, ct);
            return Ok(result);
        }

        // DELETE
        [HttpDelete("{id}")]
        [EndpointSummary("Xóa comment và toàn bộ replies (cascade)")]
        [EndpointDescription("Nếu comment có replies, hệ thống sẽ xóa tất cả.")]
        public async Task<IActionResult> Delete(
            ulong id,
            CancellationToken ct = default)
        {
            await _service.DeleteAsync(id, ct);
            return Ok(new { message = "Deleted recursively" });
        }

        // CHANGE STATUS
        [HttpPatch("{id}/status")]
        [EndpointSummary("Đổi trạng thái comment")]
        [EndpointDescription("Visible, Hidden, Deleted để phục vụ kiểm duyệt.")]
        public async Task<IActionResult> ChangeStatus(
            ulong id,
            [FromQuery] ForumCommentStatus status,
            CancellationToken ct = default)
        {
            await _service.ChangeStatusAsync(id, status, ct);
            return Ok();
        }

        // LIKE
        [HttpPost("{id}/like")]
        [EndpointSummary("Like comment")]
        [EndpointDescription("Tăng LikeCount cho comment.")]
        public async Task<IActionResult> Like(ulong id, CancellationToken ct = default)
        {
            await _service.IncrementLikeAsync(id, ct);
            return Ok();
        }

        // DISLIKE
        [HttpPost("{id}/dislike")]
        [EndpointSummary("Dislike comment")]
        [EndpointDescription("Tăng DislikeCount cho comment.")]
        public async Task<IActionResult> Dislike(ulong id, CancellationToken ct = default)
        {
            await _service.IncrementDislikeAsync(id, ct);
            return Ok();
        }
    }
}
