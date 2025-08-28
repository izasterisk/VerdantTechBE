using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO.Auth;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Routing;

namespace Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var response = new APIResponse();
            
            try
            {
                if (!ModelState.IsValid)
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(response);
                }

                var result = await _authService.LoginAsync(loginDto);
                
                response.Status = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Data = result;
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle specific exception types for different status codes
                if (ex.Message.Contains("Invalid email or password"))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Errors.Add(ex.Message);
                    return Unauthorized(response);
                }
                else if (ex.Message.Contains("Email not verified"))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.Forbidden;
                    response.Errors.Add(ex.Message);
                    return StatusCode(403, response);
                }
                else
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Errors.Add(ex.Message);
                    return StatusCode(500, response);
                }
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
        public async Task<IActionResult> SendVerification([FromBody] SendVerificationDTO dto)
        {
            var response = new APIResponse();
            
            try
            {
                if (!ModelState.IsValid)
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(response);
                }

                await _authService.SendVerificationEmailAsync(dto.Email);
                
                response.Status = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Data = "Verification email sent";
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User not found"))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add(ex.Message);
                    return NotFound(response);
                }
                else
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Errors.Add(ex.Message);
                    return StatusCode(500, response);
                }
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
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            var response = new APIResponse();
            
            try
            {
                if (!ModelState.IsValid)
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(response);
                }

                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                
                response.Status = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Data = result;
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid or expired refresh token"))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Errors.Add(ex.Message);
                    return Unauthorized(response);
                }
                else
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Errors.Add(ex.Message);
                    return StatusCode(500, response);
                }
            }
        }

        /// <summary>
        /// Get current user profile information from JWT token
        /// </summary>
        /// <returns>User information from JWT token</returns>
        [HttpGet("profile")]
        [Authorize]
        [EndpointSummary("Get User Profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var profileInfo = new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Role = role,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            var response = new APIResponse
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = profileInfo,
                Errors = new List<string>()
            };

            return Ok(response);
        }

        /// <summary>
        /// Logout endpoint - invalidates refresh token
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [Authorize]
        [EndpointSummary("User Logout (note.)")]
        [EndpointDescription("Gọi đến endpoint này để logout trước, sau đó mới xóa token trong localStorage")]
        public async Task<IActionResult> Logout()
        {
            var response = new APIResponse();
            
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Errors.Add("User not authenticated");
                    return Unauthorized(response);
                }

                // Convert string to ulong
                if (!ulong.TryParse(userIdClaim, out ulong userId))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors.Add("Invalid user ID format");
                    return BadRequest(response);
                }

                await _authService.LogoutAsync(userId);
                
                response.Status = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Data = "Logged out successfully";
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("User not found"))
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add(ex.Message);
                    return NotFound(response);
                }
                else
                {
                    response.Status = false;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Errors.Add(ex.Message);
                    return StatusCode(500, response);
                }
            }
        }
    }
}
