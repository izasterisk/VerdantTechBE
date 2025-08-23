using BLL.DTO;
using BLL.DTO.User;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<UserReadOnlyDTO> CreateUserAsync(UserCreateDTO dto);
    Task<UserReadOnlyDTO?> GetUserByIdAsync(ulong userId);
    Task<PagedResponse<UserReadOnlyDTO>> GetAllUsersAsync(int page, int pageSize, string? role = null);
    Task<UserReadOnlyDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto);
}