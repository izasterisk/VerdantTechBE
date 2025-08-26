using BLL.DTO;
using BLL.DTO.SustainabilityCertifications;

namespace BLL.Interfaces;

public interface ISustainabilityCertificationsService
{
    Task<SustainabilityCertificationsReadOnlyDTO> CreateSustainabilityCertificationAsync(SustainabilityCertificationsCreateDTO dto);
    Task<SustainabilityCertificationsReadOnlyDTO?> GetSustainabilityCertificationByIdAsync(ulong id);
    Task<SustainabilityCertificationsReadOnlyDTO> UpdateSustainabilityCertificationAsync(ulong id, SustainabilityCertificationsUpdateDTO dto);
    Task<PagedResponse<SustainabilityCertificationsReadOnlyDTO>> GetAllSustainabilityCertificationsAsync(int page, int pageSize, string? category = null);
    Task<List<SustainabilityCertificationsCategoryReadOnlyDTO>> GetAllCategoriesAsync();
}