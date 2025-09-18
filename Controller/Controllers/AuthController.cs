using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO.Auth;
using BLL.DTO;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("User Login")]
    public async Task<ActionResult<APIResponse>> Login([FromBody] LoginDTO loginDto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var result = await _authService.LoginAsync(loginDto, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Google login endpoint
    /// </summary>
    /// <param name="googleLoginDto">Google ID token</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("google-login")]
    [AllowAnonymous]
    [EndpointSummary("Google Login")]
    public async Task<ActionResult<APIResponse>> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var result = await _authService.GoogleLoginAsync(googleLoginDto, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Send verification email with 8-digit code
    /// </summary>
    /// <param name="dto">Email to send verification to</param>
    /// <returns>Confirmation</returns>
    [HttpPost("send-verification")]
    [AllowAnonymous]
    [EndpointSummary("Send Verification Email")]
    public async Task<ActionResult<APIResponse>> SendVerification([FromBody] SendEmailDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            await _authService.SendVerificationEmailAsync(dto.Email, GetCancellationToken());
            return SuccessResponse("Verification email sent");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Verify email using 8-digit code
    /// </summary>
    /// <param name="dto">Email and verification code</param>
    /// <returns>Verification confirmation</returns>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EndpointSummary("Verify Email")]
    public async Task<ActionResult<APIResponse>> VerifyEmail([FromBody] VerifyEmailDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            await _authService.VerifyEmailAsync(dto.Email, dto.Code, GetCancellationToken());
            return SuccessResponse("Email verified successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token DTO</param>
    /// <returns>New JWT token and refresh token</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EndpointSummary("Refresh Token")]
    public async Task<ActionResult<APIResponse>> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get current user profile information from JWT token
    /// </summary>
    /// <returns>User information from JWT token</returns>
    [HttpGet("profile")]
    [Authorize]
    [EndpointSummary("Get User Profile")]
    public ActionResult<APIResponse> GetProfile()
    {
        var profileInfo = new
        {
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            Name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };
        return SuccessResponse(profileInfo);
    }

    /// <summary>
    /// Send forgot password email with 8-character code
    /// </summary>
    /// <param name="dto">Email to send forgot password code to</param>
    /// <returns>Confirmation</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EndpointSummary("Send Forgot Password Email")]
    public async Task<ActionResult<APIResponse>> ForgotPassword([FromBody] SendEmailDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            await _authService.SendForgotPasswordEmailAsync(dto.Email, GetCancellationToken());
            return SuccessResponse("Forgot password email sent successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Reset password using email, code, and new password
    /// </summary>
    /// <param name="dto">Email, code, and new password</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EndpointSummary("Reset Forgot Password")]
    public async Task<ActionResult<APIResponse>> ResetPassword([FromBody] ResetForgotPasswordDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            await _authService.UpdateForgotPasswordAsync(dto.Email, dto.NewPassword, dto.Code, GetCancellationToken());
            return SuccessResponse("Password reset successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="dto">Current password and new password</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("change-password")]
    [Authorize]
    [EndpointSummary("Change Password")]
    public async Task<ActionResult<APIResponse>> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            await _authService.ChangePassword(dto.Email, dto.OldPassword, dto.NewPassword, GetCancellationToken());
            return SuccessResponse("Password changed successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Logout endpoint - invalidates refresh token
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [EndpointSummary("User Logout (note.)")]
    [EndpointDescription("Gọi đến endpoint này để logout trước, sau đó mới xóa token trong localStorage")]
    public async Task<ActionResult<APIResponse>> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
                return ErrorResponse("User not authenticated", HttpStatusCode.Unauthorized);

            if (!ulong.TryParse(userIdClaim, out ulong userId))
                return ErrorResponse("Invalid user ID format", HttpStatusCode.BadRequest);

            await _authService.LogoutAsync(userId, GetCancellationToken());
            return SuccessResponse("Logged out successfully");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}