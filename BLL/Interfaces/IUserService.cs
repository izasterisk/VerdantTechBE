using BLL.DTO;
using BLL.DTO.User;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<UserResponseDTO> CreateUserAsync(UserCreateDTO dto, CancellationToken cancellationToken = default);
    Task<UserResponseDTO> CreateStaffAsync(StaffCreateDTO dto, CancellationToken cancellationToken = default);
    Task<UserResponseDTO?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserResponseDTO>> GetAllUsersAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
    Task<UserResponseDTO> UpdateUserAsync(ulong userId, UserUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<UserResponseDTO> CreateUserAddressAsync(ulong userId, UserAddressCreateDTO dto, CancellationToken cancellationToken = default);
    Task<UserResponseDTO> UpdateUserAddressByAddressIdAsync(ulong addressId, UserAddressUpdateDTO dto, CancellationToken cancellationToken = default);
}