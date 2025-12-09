using DAL.Data.Models;

namespace DAL.IRepository;

public interface ISurveyResponseRepository
{
    Task<List<SurveyResponse>> CreateListSurveyResponsesWithTransactionAsync(List<SurveyResponse> surveyResponsesCreate, List<SurveyResponse> surveyResponsesDelete, CancellationToken cancellationToken = default);
    Task<bool> CheckIfFarmAlreadyHasSurvey(ulong farmId, CancellationToken cancellationToken);
    Task<List<SurveyResponse>> GetAllSurveyResponsesByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default);
}