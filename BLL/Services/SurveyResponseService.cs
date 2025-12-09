using AutoMapper;
using BLL.DTO.SurveyResponse;
using BLL.Interfaces;
using DAL.Data.Models;
using DAL.IRepository;

namespace BLL.Services;

public class SurveyResponseService : ISurveyResponseService
{
    private readonly IMapper _mapper;
    private readonly ISurveyResponseRepository _surveyResponseRepository;
    private readonly IFarmProfileRepository _farmProfileRepository;
    
    public SurveyResponseService(IMapper mapper, ISurveyResponseRepository surveyResponseRepository, IFarmProfileRepository farmProfileRepository)
    {
        _mapper = mapper;
        _surveyResponseRepository = surveyResponseRepository;
        _farmProfileRepository = farmProfileRepository;
    }
    
    public async Task<List<SurveyResponseDTO>> CreateSurveyResponsesAsync(ulong userId, SurveyResponseCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var check = await _farmProfileRepository.CheckIfFarmProfileBelongToUserAsync(userId, dto.FarmProfileId, cancellationToken);
        if (!check)
            throw new UnauthorizedAccessException($"Người dùng với ID {userId} không có quyền truy cập trang trại với ID {dto.FarmProfileId}");
        var existingResponses = await _surveyResponseRepository.GetAllSurveyResponsesByFarmIdAsync(dto.FarmProfileId, cancellationToken);
        
        var surveyResponses = new List<SurveyResponse>();
        var checkId = new HashSet<ulong>();
        foreach (var answer in dto.Answers)
        {
            if(!checkId.Add(answer.QuestionId))
                throw new ArgumentException($"Câu hỏi với ID {answer.QuestionId} bị trùng lặp trong câu trả lời.");
            surveyResponses.Add(new SurveyResponse
            {
                FarmProfileId = dto.FarmProfileId,
                QuestionId = answer.QuestionId,
                TextAnswer = answer.TextAnswer,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _surveyResponseRepository.CreateListSurveyResponsesWithTransactionAsync(surveyResponses, existingResponses, cancellationToken);
        var result = await _surveyResponseRepository.GetAllSurveyResponsesByFarmIdAsync(dto.FarmProfileId, cancellationToken);
        return _mapper.Map<List<SurveyResponseDTO>>(result);
    }

    public async Task<List<SurveyResponseDTO>> GetAllSurveyResponsesAsync(ulong userId, ulong farmId, CancellationToken cancellationToken = default)
    {
        var check = await _farmProfileRepository.CheckIfFarmProfileBelongToUserAsync(userId, farmId, cancellationToken);
        if (!check)
            throw new UnauthorizedAccessException($"Người dùng với ID {userId} không có quyền truy cập trang trại với ID {farmId}");

        var result = await _surveyResponseRepository.GetAllSurveyResponsesByFarmIdAsync(farmId, cancellationToken);
        return _mapper.Map<List<SurveyResponseDTO>>(result);
    }
}