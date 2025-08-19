using BLL.DTO;
using BLL.DTO.Auth;

namespace BLL.Interfaces;

public interface IAuthService
{
    Task<APIResponse> LoginAsync(LoginDTO loginDto);
    Task<APIResponse> ValidateTokenAsync(string token);
    Task<APIResponse> RefreshTokenAsync(string refreshToken);
}