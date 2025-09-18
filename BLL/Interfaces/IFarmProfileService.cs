using BLL.DTO.FarmProfile;

namespace BLL.Interfaces
{
    public interface IFarmProfileService
    {
        Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken cancellationToken = default);
        Task<FarmProfileResponseDTO?> GetAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
        Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(ulong id, ulong currentUserId, CancellationToken cancellationToken = default);
    }
}