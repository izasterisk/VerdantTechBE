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
    /// Lấy dữ liệu đất theo Farm ID
    /// </summary>
    /// <param name="farmId">ID của trang trại</param>
    /// <returns>Dữ liệu đất của trang trại</returns>
    [HttpGet("farm/{farmId}/soil")]
    [Authorize]
    [EndpointSummary("Get Soil Data by Farm ID")]
    [EndpointDescription("Sẽ trả về 3 giá trị theo thứ tự là 0-5cm, 5-15cm, 15-30cm.")]
    public async Task<ActionResult<APIResponse>> GetSoilDataByFarmId(ulong farmId)
    {
        try
        {
            var soilData = await _co2Service.GetSoilDataByFarmIdAsync(farmId, GetCancellationToken());
            return SuccessResponse(soilData);
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
    [EndpointDescription("Đây là HARD DELETE, xóa là mất luôn.")]
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