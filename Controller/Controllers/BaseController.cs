using Microsoft.AspNetCore.Mvc;
using BLL.DTO;
using System.Net;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BLL.Helpers.Order;
using System.Security;

namespace Controller.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<APIResponse>? ValidateModel()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(APIResponse.ValidationError(errors));
        }
        return null;
    }

    // bóc tách message có ích từ inner exception (nếu có)
    private static string GetRootMessage(Exception ex)
    {
        var e = ex;
        while (e.InnerException != null) e = e.InnerException;

        var msg = e.Message;

        // gợi ý mapping nội dung thường gặp (FK/UK/duplicate)
        if (msg.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            return "Vi phạm khóa ngoại.";
        if (msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase))
            return "Dữ liệu trùng lặp (vi phạm unique).";

        return string.IsNullOrWhiteSpace(msg) ? ex.Message : msg;
    }

    protected ActionResult<APIResponse> HandleException(Exception ex)
    {
        string rootMsg = GetRootMessage(ex);
        string safeMsg = string.IsNullOrWhiteSpace(ex.Message) ? rootMsg : ex.Message;

        return ex switch
        {
            // 401
            UnauthorizedAccessException =>
                Unauthorized(APIResponse.Error("Truy cập bị từ chối", HttpStatusCode.Unauthorized)),

            // 403
            SecurityException =>
                StatusCode(403, APIResponse.Error("Không có quyền truy cập tài nguyên này.", HttpStatusCode.Forbidden)),

            // 404
            KeyNotFoundException =>
                NotFound(APIResponse.Error(safeMsg, HttpStatusCode.NotFound)),

            // 409 (xung đột ghi/đồng bộ)
            DbUpdateConcurrencyException =>
                StatusCode(409, APIResponse.Error("Xung đột dữ liệu. Vui lòng tải lại và thử lại.", HttpStatusCode.Conflict)),

            // 409/400 cho lỗi DB
            DbUpdateException when rootMsg.Contains("Vi phạm khóa ngoại") ||
                                   rootMsg.Contains("trùng lặp", StringComparison.OrdinalIgnoreCase) ||
                                   rootMsg.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                                   rootMsg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) =>
                StatusCode(409, APIResponse.Error(rootMsg, HttpStatusCode.Conflict)),

            DbUpdateException =>
                BadRequest(APIResponse.Error(rootMsg, HttpStatusCode.BadRequest)),

            // 400 cho input/validation/mapping
            ArgumentNullException =>
                BadRequest(APIResponse.Error("Thiếu dữ liệu bắt buộc.", HttpStatusCode.BadRequest)),

            ArgumentException =>
                BadRequest(APIResponse.Error(safeMsg, HttpStatusCode.BadRequest)),

            FormatException =>
                BadRequest(APIResponse.Error("Định dạng dữ liệu không hợp lệ.", HttpStatusCode.BadRequest)),

            ValidationException =>
                BadRequest(APIResponse.Error(safeMsg, HttpStatusCode.BadRequest)),

            InvalidDataException =>
                BadRequest(APIResponse.Error(safeMsg, HttpStatusCode.BadRequest)),

            AutoMapperMappingException =>
                BadRequest(APIResponse.Error("Dữ liệu không thể map sang mô hình đích.", HttpStatusCode.BadRequest)),

            InvalidOperationException invEx =>
                BadRequest(APIResponse.Error(
                    string.IsNullOrWhiteSpace(invEx.Message) ? "Yêu cầu không hợp lệ." : invEx.Message,
                    HttpStatusCode.BadRequest)),

            // 499/408: request bị hủy/timeout
            OperationCanceledException or TaskCanceledException =>
                StatusCode(499, APIResponse.Error("Yêu cầu đã bị hủy.", (HttpStatusCode)499)),

            HttpRequestException =>
                StatusCode(503, APIResponse.Error("Dịch vụ phụ trợ không khả dụng.", HttpStatusCode.ServiceUnavailable)),

            NotImplementedException =>
                StatusCode(501, APIResponse.Error("Chức năng chưa được triển khai.", HttpStatusCode.NotImplemented)),

            // 500 mặc định
            _ => StatusCode(500, APIResponse.Error("Lỗi máy chủ nội bộ", HttpStatusCode.InternalServerError))
        };
    }

    protected ActionResult<APIResponse> SuccessResponse(object? data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = APIResponse.Success(data ?? new object(), statusCode);
        return statusCode switch
        {
            HttpStatusCode.Created   => Created("", response),
            HttpStatusCode.NoContent => NoContent(),
            _                        => Ok(response)
        };
    }

    protected ActionResult<APIResponse> ErrorResponse(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        var response = APIResponse.Error(message, statusCode);
        return statusCode switch
        {
            HttpStatusCode.NotFound      => NotFound(response),
            HttpStatusCode.Unauthorized  => Unauthorized(response),
            HttpStatusCode.Forbidden     => StatusCode(403, response),
            HttpStatusCode.BadRequest    => BadRequest(response),
            HttpStatusCode.Conflict      => StatusCode(409, response),
            HttpStatusCode.Gone          => StatusCode(410, response),
            HttpStatusCode.ServiceUnavailable => StatusCode(503, response),
            _                             => StatusCode((int)statusCode, response)
        };
    }

    protected ulong GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực");

        if (!ulong.TryParse(userIdClaim, out ulong userId))
            throw new ArgumentException("Định dạng ID người dùng không hợp lệ");

        return userId;
    }

    protected CancellationToken GetCancellationToken() => HttpContext.RequestAborted;
}
