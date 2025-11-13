using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
[Authorize]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;
    
    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Đổi trạng thái đã đọc/chưa đọc của thông báo
    /// </summary>
    /// <param name="id">ID của thông báo</param>
    /// <returns>Thông tin thông báo đã cập nhật</returns>
    [HttpPatch("{id}/revert-read-status")]
    [EndpointSummary("Revert Read Status")]
    [EndpointDescription("Đảo ngược trạng thái đã đọc/chưa đọc của thông báo")]
    public async Task<ActionResult<APIResponse>> RevertReadStatus(ulong id)
    {
        try
        {
            var notification = await _notificationService.RevertReadStatusAsync(id, GetCancellationToken());
            return SuccessResponse(notification);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách thông báo của người dùng với phân trang
    /// </summary>
    /// <param name="userId">ID của người dùng</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách thông báo có phân trang</returns>
    [HttpGet("user/{userId}")]
    [EndpointSummary("Get All Notifications By User ID")]
    [EndpointDescription("Lấy danh sách thông báo của người dùng với phân trang. Mẫu: /api/Notification/user/1?page=1&pageSize=10")]
    public async Task<ActionResult<APIResponse>> GetAllNotificationsByUserId(ulong userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var notifications = await _notificationService.GetAllNotificationsByUserIdAsync(userId, page, pageSize, GetCancellationToken());
            return SuccessResponse(notifications);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xóa thông báo theo ID
    /// </summary>
    /// <param name="id">ID của thông báo</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [EndpointSummary("Delete Notification")]
    [EndpointDescription("Xóa thông báo theo ID")]
    public async Task<ActionResult<APIResponse>> DeleteNotification(ulong id)
    {
        try
        {
            var result = await _notificationService.DeleteNotificationAsync(id, GetCancellationToken());
            if (result)
                return SuccessResponse("Xóa thông báo thành công");
            else
                return ErrorResponse("Không thể xóa thông báo", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
