using BLL.DTO.FarmProfile;

namespace BLL.Interfaces
{
    public interface IFarmProfileService
    {
        Task<FarmProfileResponseDTO> CreateAsync(ulong currentUserId, FarmProfileCreateDto dto);
        Task<FarmProfileResponseDTO?> GetAsync(ulong id);
        Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllByUserIdAsync(ulong userId);
        Task<FarmProfileResponseDTO> UpdateAsync(ulong id, ulong currentUserId, FarmProfileUpdateDTO dto);
        Task<bool> DeleteAsync(ulong id, ulong currentUserId);
    }
}