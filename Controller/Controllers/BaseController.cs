using Microsoft.AspNetCore.Mvc;
using BLL.DTO;
using System.Net;
using System.Security.Claims;

namespace Controller.Controllers;

/// <summary>
/// Base controller với common helpers cho tất cả controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Validate ModelState và return BadRequest nếu invalid
    /// </summary>
    /// <returns>BadRequest response nếu ModelState invalid, null nếu valid</returns>
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

    /// <summary>
    /// Handle exceptions và return appropriate HTTP response
    /// </summary>
    /// <param name="ex">Exception to handle</param>
    /// <returns>ActionResult với appropriate status code</returns>
    protected ActionResult<APIResponse> HandleException(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => Unauthorized(APIResponse.Error("Truy cập bị từ chối", HttpStatusCode.Unauthorized)),
            ArgumentException => BadRequest(APIResponse.Error(ex.Message, HttpStatusCode.BadRequest)),
            InvalidOperationException when ex.Message.Contains("Email chưa được xác minh") => 
                StatusCode(403, APIResponse.Error("Email chưa được xác minh", HttpStatusCode.Forbidden)),
            InvalidOperationException when ex.Message.Contains("Không tìm thấy người dùng") => 
                NotFound(APIResponse.Error("Không tìm thấy người dùng", HttpStatusCode.NotFound)),
            InvalidOperationException => 
                BadRequest(APIResponse.Error(
                    string.IsNullOrEmpty(ex.Message) ? "Yêu cầu không hợp lệ" : ex.Message, 
                    HttpStatusCode.BadRequest)),
            _ => StatusCode(500, APIResponse.Error("Lỗi máy chủ nội bộ", HttpStatusCode.InternalServerError))
        };
    }

    /// <summary>
    /// Create success response với data
    /// </summary>
    protected ActionResult<APIResponse> SuccessResponse(object? data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = APIResponse.Success(data ?? new object(), statusCode);
        return statusCode switch
        {
            HttpStatusCode.Created => Created("", response),
            HttpStatusCode.NoContent => NoContent(),
            _ => Ok(response)
        };
    }

    /// <summary>
    /// Create error response với message và status code
    /// </summary>
    protected ActionResult<APIResponse> ErrorResponse(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        var response = APIResponse.Error(message, statusCode);
        return statusCode switch
        {
            HttpStatusCode.NotFound => NotFound(response),
            HttpStatusCode.Unauthorized => Unauthorized(response),
            HttpStatusCode.Forbidden => Forbid(),
            HttpStatusCode.BadRequest => BadRequest(response),
            _ => StatusCode((int)statusCode, response)
        };
    }

    /// <summary>
/// Lấy ID người dùng hiện tại từ JWT token
/// </summary>
/// <returns>ID người dùng hoặc throw exception nếu chưa xác thực</returns>
protected ulong GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    if (string.IsNullOrEmpty(userIdClaim))
        throw new UnauthorizedAccessException("Người dùng chưa được xác thực");

    if (!ulong.TryParse(userIdClaim, out ulong userId))
        throw new ArgumentException("Định dạng ID người dùng không hợp lệ");

    return userId;
}


    /// <summary>
    /// Get CancellationToken from HttpContext
    /// </summary>
    /// <returns>CancellationToken from request context</returns>
    protected CancellationToken GetCancellationToken()
    {
        return HttpContext.RequestAborted;
    }
}
