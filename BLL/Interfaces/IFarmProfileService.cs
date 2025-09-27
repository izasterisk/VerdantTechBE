using BLL.DTO.FarmProfile;
using DAL.Data.Models;

namespace BLL.Interfaces
{
    public interface IFarmProfileService
    {
        Task<FarmProfileResponseDTO> CreateFarmProfileAsync(ulong currentUserId, FarmProfileCreateDto dto, CancellationToken cancellationToken = default);
        Task<FarmProfileResponseDTO?> GetFarmProfileByFarmIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FarmProfileResponseDTO>> GetAllFarmProfileByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
        Task<FarmProfileResponseDTO> UpdateFarmProfileAsync(ulong id, FarmProfileUpdateDTO dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteByChangeStatusFarmProfileAsync(ulong id, CancellationToken cancellationToken = default);
    }
}