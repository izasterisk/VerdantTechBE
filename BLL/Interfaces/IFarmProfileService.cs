using BLL.DTO.FarmProfile;

namespace BLL.Interfaces
{
    public interface IFarmProfileService
    {
        Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken ct = default);

        Task<FarmProfileResponseDTO?> GetAsync(ulong id, ulong currentUserId, CancellationToken ct = default);

        Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId, CancellationToken ct = default);

        Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileUpdateDTO dto, CancellationToken ct = default);
        /// <summary>Hard delete.</summary>
        Task<bool> DeleteAsync(ulong id, ulong currentUserId, CancellationToken ct = default);
    }
}