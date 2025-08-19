using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO.Auth;
using BLL.DTO;
using System.Net;

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
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorResponse = new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = null!,
                    Errors = errors
                };

                return BadRequest(errorResponse);
            }

            var result = await _authService.LoginAsync(loginDto);
            
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.Unauthorized => Unauthorized(result),
                HttpStatusCode.Forbidden => StatusCode(403, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Token validation result</returns>
        [HttpPost("validate-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var errorResponse = new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = null!,
                    Errors = new List<string> { "Token is required" }
                };

                return BadRequest(errorResponse);
            }

            var result = await _authService.ValidateTokenAsync(token);
            
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.Unauthorized => Unauthorized(result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                var errorResponse = new APIResponse
                {
                    Status = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Data = null!,
                    Errors = new List<string> { "Refresh token is required" }
                };

                return BadRequest(errorResponse);
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);
            
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(result),
                HttpStatusCode.Unauthorized => Unauthorized(result),
                HttpStatusCode.NotImplemented => StatusCode(501, result),
                _ => StatusCode(500, result)
            };
        }

        /// <summary>
        /// Test protected endpoint
        /// </summary>
        /// <returns>User information from JWT token</returns>
        [HttpGet("profile")]
        [Authorize]
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
    }
}
