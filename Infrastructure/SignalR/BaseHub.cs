using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Infrastructure.SignalR;

/// <summary>
/// Base class cho tất cả SignalR Hubs
/// Cung cấp các helper methods giống BaseController
/// </summary>
public abstract class BaseHub : Hub
{
    /// <summary>
    /// Lấy UserId từ JWT token claims
    /// Logic giống BaseController.GetCurrentUserId()
    /// </summary>
    protected ulong GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực");

        if (!ulong.TryParse(userIdClaim, out ulong userId))
            throw new ArgumentException("Định dạng ID người dùng không hợp lệ");

        return userId;
    }

    /// <summary>
    /// Thử lấy UserId, trả về null nếu không có
    /// </summary>
    protected ulong? TryGetCurrentUserId()
    {
        try
        {
            return GetCurrentUserId();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lấy Role của user hiện tại
    /// </summary>
    protected string? GetCurrentUserRole()
    {
        return Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    }
}
