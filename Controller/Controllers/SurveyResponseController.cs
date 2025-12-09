using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.SurveyResponse;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class SurveyResponseController : BaseController
{
    private readonly ISurveyResponseService _surveyResponseService;
    
    public SurveyResponseController(ISurveyResponseService surveyResponseService)
    {
        _surveyResponseService = surveyResponseService;
    }

    /// <summary>
    /// Tạo hoặc cập nhật câu trả lời khảo sát bền vững cho trang trại
    /// </summary>
    /// <param name="dto">Thông tin câu trả lời khảo sát</param>
    /// <returns>Danh sách câu trả lời đã tạo</returns>
    [HttpPost]
    [Authorize(Roles = "Customer,Vendor")]
    [EndpointSummary("Create or Update Survey Responses")]
    [EndpointDescription("Nếu đã có khảo sát cũ, sẽ tự động xóa và thay thế bằng khảo sát mới. " +
                         "Phải trả lời đủ 10 câu hỏi và không được trùng lặp QuestionId.")]
    public async Task<ActionResult<APIResponse>> CreateSurveyResponses([FromBody] SurveyResponseCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var responses = await _surveyResponseService.CreateSurveyResponsesAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(responses, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả câu trả lời khảo sát của một trang trại
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <returns>Danh sách câu trả lời khảo sát</returns>
    [HttpGet("farm/{farmId}")]
    [Authorize(Roles = "Customer,Vendor")]
    [EndpointSummary("Get All Survey Responses By Farm ID")]
    [EndpointDescription("Lấy tất cả 10 câu trả lời khảo sát của trang trại, sắp xếp theo QuestionId tăng dần. " +
                         "Chỉ chủ trang trại mới có quyền xem.")]
    public async Task<ActionResult<APIResponse>> GetAllSurveyResponses(ulong farmId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var responses = await _surveyResponseService.GetAllSurveyResponsesAsync(userId, farmId, GetCancellationToken());
            return SuccessResponse(responses);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
