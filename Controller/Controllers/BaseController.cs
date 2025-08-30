using Microsoft.AspNetCore.Mvc;
using BLL.DTO;
using System.Net;

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
            ArgumentException => BadRequest(APIResponse.Error("Yêu cầu không hợp lệ", HttpStatusCode.BadRequest)),
            InvalidOperationException when ex.Message.Contains("Email chưa được xác minh") => 
                StatusCode(403, APIResponse.Error("Email chưa được xác minh", HttpStatusCode.Forbidden)),
            InvalidOperationException => 
                ex.Message.Contains("Không tìm thấy người dùng") 
                    ? NotFound(APIResponse.Error("Không tìm thấy người dùng", HttpStatusCode.NotFound))
                    : BadRequest(APIResponse.Error("Yêu cầu không hợp lệ", HttpStatusCode.BadRequest)),
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
}
