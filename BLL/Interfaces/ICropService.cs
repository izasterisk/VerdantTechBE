using BLL.DTO.Crops;
using BLL.DTO.FarmProfile;

namespace BLL.Interfaces
{
    public interface ICropService
    {
        Task<FarmProfileResponseDTO> AddCropsToFarmAsync(ulong farmId, List<CropsCreateDTO> dtos, CancellationToken cancellationToken = default);
        Task<FarmProfileResponseDTO> UpdateCropsAsync(ulong farmId, List<CropsUpdateDTO> dtos, CancellationToken cancellationToken = default);
    }
}