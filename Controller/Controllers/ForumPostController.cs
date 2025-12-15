using BLL.DTO.ForumPost;
using BLL.DTO.MediaLink;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;

namespace Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumPostController : ControllerBase
    {
        private readonly IForumPostService _forumPostService;
        private readonly ICloudinaryService _cloudinary; 

        public ForumPostController(
            IForumPostService forumPostService,
            ICloudinaryService cloudinary
            )
        {
            _forumPostService = forumPostService;
            _cloudinary = cloudinary;
        }

        // ===================== GET ALL =====================
        [HttpGet]
        [EndpointSummary("Lấy danh sách bài viết forum")]
        [EndpointDescription("Lấy toàn bộ forum posts có phân trang.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _forumPostService.GetAllAsync(page, pageSize, ct);
            return Ok(result);
        }

        // ===================== GET ALL BY CATEGORY =====================
        [HttpGet("category/{categoryId}")]
        [EndpointSummary("Lấy danh sách bài viết theo danh mục")]
        [EndpointDescription("Lấy forum posts thuộc một ForumCategory cụ thể, có phân trang.")]
        public async Task<IActionResult> GetAllByCategoryId(
            ulong categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _forumPostService.GetAllByCategoryIdAsync(categoryId, page, pageSize, ct);
            return Ok(result);
        }

        // ===================== GET DETAIL =====================
        [HttpGet("{id}")]
        [EndpointSummary("Lấy chi tiết một bài viết")]
        [EndpointDescription("Trả về chi tiết bài viết, bao gồm content blocks, images, và danh sách comments (ở dạng object).")]
        public async Task<IActionResult> GetDetail(
            ulong id,
            CancellationToken ct = default)
        {
            var result = await _forumPostService.GetDetailAsync(id, ct);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // ===================== GET POST WITH COMMENTS (DEEP) =====================
        [HttpGet("{id}/with-comments")]
        [EndpointSummary("Lấy bài viết cùng toàn bộ comments")]
        [EndpointDescription("Trả về bài viết kèm danh sách comments. Sau này có thể map thành nested comment DTO.")]
        public async Task<IActionResult> GetPostWithComments(
            ulong id,
            CancellationToken ct = default)
        {
            var result = await _forumPostService.GetPostWithCommentsAsync(id, ct);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // ===================== CREATE =====================
        [HttpPost]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Tạo mới bài viết forum")]
        [EndpointDescription("Tạo mới bài viết, hỗ trợ nhiều ContentBlock và upload nhiều ảnh (files nằm trong FormData).")]
        public async Task<IActionResult> Create(
            [FromForm] ForumPostCreateDTO dto,
            [FromQuery] ulong userId,
            CancellationToken ct = default)
        {
            var addImages = new List<MediaLinkItemDTO>();

            if (dto.AddImages != null && dto.AddImages.Any())
            {
                var uploads = await _cloudinary.UploadManyAsync(dto.AddImages, "forum_posts", ct);

                addImages = uploads.Select((u, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = u.PublicId,
                    ImageUrl = u.PublicUrl,
                    SortOrder = i,
                    Purpose = MediaPurpose.Front.ToString()
                }).ToList();
            }

            var result = await _forumPostService.CreateAsync(userId, dto, addImages, ct);
            return Ok(result);
        }

        // ===================== UPDATE =====================
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [EndpointSummary("Cập nhật bài viết forum")]
        [EndpointDescription("Cập nhật bài viết, cho phép chỉnh sửa nội dung, tags, content blocks, và xử lý thêm/xóa ảnh.")]
        public async Task<IActionResult> Update(
            ulong id,
            [FromForm] ForumPostUpdateDTO dto,
            CancellationToken ct = default)
        {
            var addImages = new List<MediaLinkItemDTO>();
            var removedImages = dto.RemoveImagePublicIds?.ToList() ?? new List<string>();

            if (dto.AddImages != null && dto.AddImages.Any())
            {
                var uploads = await _cloudinary.UploadManyAsync(dto.AddImages, "forum/posts", ct);

                addImages = uploads.Select((u, i) => new MediaLinkItemDTO
                {
                    ImagePublicId = u.PublicId,
                    ImageUrl = u.PublicUrl,
                    SortOrder = i
                }).ToList();
            }

            var result = await _forumPostService.UpdateAsync(id, dto, addImages, removedImages, ct);
            return Ok(result);
        }

        // ===================== DELETE =====================
        [HttpDelete("{id}")]
        [EndpointSummary("Xóa bài viết forum")]
        [EndpointDescription("Xóa một bài viết theo Id.")]
        public async Task<IActionResult> Delete(
            ulong id,
            CancellationToken ct = default)
        {
            await _forumPostService.DeleteAsync(id, ct);
            return Ok(new { message = "Deleted successfully" });
        }

        // ===================== PIN / UNPIN =====================
        [HttpPatch("{id}/pin")]
        [EndpointSummary("Pin / Unpin bài viết")]
        [EndpointDescription("Cập nhật trạng thái IsPinned của bài viết.")]
        public async Task<IActionResult> Pin(
            ulong id,
            [FromQuery] bool isPinned,
            CancellationToken ct = default)
        {
            await _forumPostService.PinAsync(id, isPinned, ct);
            return Ok(new { message = "Pin state updated" });
        }

        // ===================== CHANGE STATUS =====================
        [HttpPatch("{id}/status")]
        [EndpointSummary("Thay đổi trạng thái hiển thị bài viết")]
        [EndpointDescription("Đổi trạng thái bài viết giữa Visible / Hidden.")]
        public async Task<IActionResult> ChangeStatus(
            ulong id,
            [FromQuery] ForumPostStatus status,
            CancellationToken ct = default)
        {
            await _forumPostService.ChangeStatusAsync(id, status, ct);
            return Ok(new { message = "Status updated" });
        }

        // ===================== VIEW COUNT =====================
        [HttpPost("{id}/view")]
        [EndpointSummary("Tăng view cho bài viết")]
        [EndpointDescription("Tăng ViewCount của bài viết, thường gọi khi người dùng mở chi tiết bài viết.")]
        public async Task<IActionResult> IncreaseView(
            ulong id,
            CancellationToken ct = default)
        {
            await _forumPostService.IncrementViewAsync(id, ct);
            return Ok();
        }

        // ===================== LIKE =====================
        [HttpPost("{id}/like")]
        [EndpointSummary("Tăng like cho bài viết")]
        [EndpointDescription("Tăng LikeCount của bài viết.")]
        public async Task<IActionResult> IncreaseLike(
            ulong id,
            CancellationToken ct = default)
        {
            await _forumPostService.IncrementLikeAsync(id, ct);
            return Ok();
        }

        // ===================== DISLIKE =====================
        [HttpPost("{id}/dislike")]
        [EndpointSummary("Tăng dislike cho bài viết")]
        [EndpointDescription("Tăng DislikeCount của bài viết.")]
        public async Task<IActionResult> IncreaseDislike(
            ulong id,
            CancellationToken ct = default)
        {
            await _forumPostService.IncrementDislikeAsync(id, ct);
            return Ok();
        }
    }
}
