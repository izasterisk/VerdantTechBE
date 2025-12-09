using BLL.DTO.SurveyResponse;

namespace BLL.Interfaces;

public interface ISurveyResponseService
{
    Task<List<SurveyResponseDTO>> CreateSurveyResponsesAsync(ulong userId, SurveyResponseCreateDTO dto, CancellationToken cancellationToken = default);
    Task<List<SurveyResponseDTO>> GetAllSurveyResponsesAsync(ulong userId, ulong farmId, CancellationToken cancellationToken = default);
}