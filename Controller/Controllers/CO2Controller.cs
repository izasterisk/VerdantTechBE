using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.CO2;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class CO2Controller : BaseController
{
    private readonly ICO2Service _co2Service;
    
    public CO2Controller(ICO2Service co2Service)
    {
        _co2Service = co2Service;
    }

    /// <summary>
    /// Tạo CO2 footprint cho trang trại
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <param name="dto">Thông tin CO2 footprint cần tạo</param>
    /// <returns>Thông tin CO2 footprint đã tạo</returns>
    [HttpPost("farm/{farmId}")]
    [Authorize]
    [EndpointSummary("Create CO2 Footprint for Farm")]
    [EndpointDescription("Tạo dữ liệu CO2 footprint cho trang trại trong khoảng thời gian xác định")]
    public async Task<ActionResult<APIResponse>> CreateCO2Footprint(ulong farmId, [FromBody] CO2FootprintCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var co2Data = await _co2Service.CreateCO2FootprintAsync(farmId, dto, GetCancellationToken());
            return SuccessResponse(co2Data, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy tất cả dữ liệu môi trường theo Farm ID
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <returns>Danh sách dữ liệu môi trường của trang trại</returns>
    [HttpGet("farm/{farmId}")]
    [Authorize]
    [EndpointSummary("Get All Environmental Data by Farm ID")]
    [EndpointDescription("Lấy tất cả dữ liệu môi trường của một trang trại")]
    public async Task<ActionResult<APIResponse>> GetAllEnvironmentDataByFarmId(ulong farmId)
    {
        try
        {
            var environmentalData = await _co2Service.GetAllEnvironmentDataByFarmIdAsync(farmId, GetCancellationToken());
            return SuccessResponse(environmentalData);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy dữ liệu môi trường theo ID
    /// </summary>
    /// <param name="id">ID của dữ liệu môi trường</param>
    /// <returns>Thông tin dữ liệu môi trường</returns>
    [HttpGet("{id}")]
    [Authorize]
    [EndpointSummary("Get Environmental Data By ID")]
    [EndpointDescription("Lấy thông tin chi tiết của một bản ghi dữ liệu môi trường")]
    public async Task<ActionResult<APIResponse>> GetEnvironmentDataById(ulong id)
    {
        try
        {
            var environmentalData = await _co2Service.GetEnvironmentDataByIdAsync(id, GetCancellationToken());
            
            if (environmentalData == null)
                return ErrorResponse($"Không tìm thấy dữ liệu môi trường với ID {id}", HttpStatusCode.NotFound);

            return SuccessResponse(environmentalData);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xóa dữ liệu môi trường theo ID
    /// </summary>
    /// <param name="id">ID của dữ liệu môi trường cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [EndpointSummary("Delete Environmental Data")]
    [EndpointDescription("Xóa một bản ghi dữ liệu môi trường và các dữ liệu liên quan")]
    public async Task<ActionResult<APIResponse>> DeleteEnvironmentalData(ulong id)
    {
        try
        {
            var result = await _co2Service.DeleteEnvironmentalDataByIdAsync(id, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}