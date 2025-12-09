using DAL.Data;
using DAL.Data.Models;
using DAL.IRepository;

namespace DAL.Repository;

public class SurveyResponseRepository : ISurveyResponseRepository
{
    private readonly IRepository<SurveyResponse> _surveyResponseRepository;
    private readonly IRepository<FarmProfile> _farmProfileRepository; 
    private readonly VerdantTechDbContext _dbContext;
    
    public SurveyResponseRepository(IRepository<SurveyResponse> surveyResponseRepository,
        IRepository<FarmProfile> farmProfileRepository, VerdantTechDbContext dbContext)
    {
        _surveyResponseRepository = surveyResponseRepository;
        _farmProfileRepository = farmProfileRepository;
        _dbContext = dbContext;
    }
    
    public async Task<List<SurveyResponse>> CreateListSurveyResponsesAsync(List<SurveyResponse> surveyResponses, CancellationToken cancellationToken = default)
    {
        return await _surveyResponseRepository.CreateBulkAsync(surveyResponses, cancellationToken);
    }
    
    public async Task DeleteAllSurveyResponsesByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        var survey = await _surveyResponseRepository.GetAllByFilterAsync
            (s => s.FarmProfileId == farmId, true, cancellationToken);
        await _surveyResponseRepository.DeleteBulkAsync(survey, cancellationToken);
    }
    
    public async Task<List<SurveyResponse>> GetAllSurveyResponsesByFarmIdAsync(ulong farmId, CancellationToken cancellationToken = default)
    {
        var responses = await _surveyResponseRepository.GetAllByFilterAsync(
            s => s.FarmProfileId == farmId, 
            useNoTracking: true, 
            cancellationToken);
        return responses.OrderBy(s => s.QuestionId).ToList();
    }
    
    public async Task<bool> CheckIfFarmAlreadyHasSurvey(ulong farmId, CancellationToken cancellationToken = default) =>
        await _surveyResponseRepository.AnyAsync(s => s.FarmProfileId == farmId, cancellationToken);
}