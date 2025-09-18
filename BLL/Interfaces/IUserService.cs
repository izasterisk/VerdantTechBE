using BLL.DTO;
using BLL.DTO.User;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<UserReadOnlyDTO> CreateUserAsync(UserCreateDTO dto, CancellationToken cancellationToken = default);
    Task<UserReadOnlyDTO?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserReadOnlyDTO>> GetAllUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
    Task<UserReadOnlyDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto, CancellationToken cancellationToken = default);
}