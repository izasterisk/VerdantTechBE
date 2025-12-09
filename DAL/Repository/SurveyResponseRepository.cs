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
    
    public async Task<List<SurveyResponse>> CreateListSurveyResponsesWithTransactionAsync(List<SurveyResponse> surveyResponsesCreate, 
        List<SurveyResponse> surveyResponsesDelete, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (surveyResponsesDelete.Count > 0)
            {
                await _surveyResponseRepository.DeleteBulkAsync(surveyResponsesDelete, cancellationToken);
            }
            var result = await _surveyResponseRepository.CreateBulkAsync(surveyResponsesCreate, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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