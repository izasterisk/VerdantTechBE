using BLL.DTO;
using BLL.DTO.User;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<UserReadOnlyDTO> CreateUserAsync(UserCreateDTO dto);
    Task<UserReadOnlyDTO?> GetUserByIdAsync(ulong userId);
    Task<PagedResponse<UserReadOnlyDTO>> GetAllUsersAsync(int page, int pageSize);
    Task<UserReadOnlyDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto);
}