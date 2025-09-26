using BLL.DTO.CO2;

namespace BLL.Interfaces;

public interface ICO2Service
{
    Task<CO2FootprintResponseDTO> CreateCO2FootprintAsync(ulong farmId, CO2FootprintCreateDTO dto, CancellationToken cancellationToken = default);
    Task<List<CO2FootprintResponseDTO>> GetAllEnvironmentDataByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task<CO2FootprintResponseDTO?> GetEnvironmentDataByIdAsync(ulong id, CancellationToken cancellationToken = default);
    Task<string> DeleteEnvironmentalDataByIdAsync(ulong id, CancellationToken cancellationToken = default);
}