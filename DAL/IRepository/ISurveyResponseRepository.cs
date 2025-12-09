using DAL.Data.Models;

namespace DAL.IRepository;

public interface ISurveyResponseRepository
{
    Task<List<SurveyResponse>> CreateListSurveyResponsesAsync(List<SurveyResponse> surveyResponses, CancellationToken cancellationToken = default);
    Task DeleteAllSurveyResponsesByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
    Task<bool> CheckIfFarmAlreadyHasSurvey(ulong farmId, CancellationToken cancellationToken);
    Task<List<SurveyResponse>> GetAllSurveyResponsesByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
}