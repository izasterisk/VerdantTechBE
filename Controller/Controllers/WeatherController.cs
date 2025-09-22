using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Weather;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
[Authorize]
public class WeatherController : BaseController
{
    private readonly IWeatherService _weatherService;
    
    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Lấy thông tin thời tiết theo giờ của nông trại (ngày hôm nay)
    /// </summary>
    /// <param name="farmId">ID của nông trại</param>
    /// <returns>Thông tin thời tiết theo giờ</returns>
    [HttpGet("hourly/{farmId}")]
    [EndpointSummary("Get Hourly Weather Details By Farm ID")]
    [EndpointDescription("Lấy thông tin thời tiết theo giờ của nông trại cho ngày hôm nay dựa trên tọa độ địa chỉ nông trại.")]
    public async Task<ActionResult<APIResponse>> GetHourlyWeatherDetailsByFarmId(ulong farmId)
    {
        try
        {
            var weatherData = await _weatherService.GetHourlyWeatherDetailsByFarmIdAsync(farmId, GetCancellationToken());
            return SuccessResponse(weatherData, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin thời tiết theo ngày của nông trại (7 ngày từ hôm nay)
    /// </summary>
    /// <param name="farmId">ID của nông trại</param>
    /// <returns>Thông tin thời tiết theo ngày</returns>
    [HttpGet("daily/{farmId}")]
    [EndpointSummary("Get Daily Weather Details By Farm ID")]
    [EndpointDescription("Lấy thông tin thời tiết theo ngày của nông trại cho 7 ngày từ hôm nay dựa trên tọa độ địa chỉ nông trại.")]
    public async Task<ActionResult<APIResponse>> GetDailyWeatherDetailsByFarmId(ulong farmId)
    {
        try
        {
            var weatherData = await _weatherService.GetDailyWeatherDetailsByFarmIdAsync(farmId, GetCancellationToken());
            return SuccessResponse(weatherData, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin thời tiết hiện tại của nông trại
    /// </summary>
    /// <param name="farmId">ID của nông trại</param>
    /// <returns>Thông tin thời tiết hiện tại</returns>
    [HttpGet("current/{farmId}")]
    [EndpointSummary("Get Current Weather Details By Farm ID")]
    [EndpointDescription("Lấy thông tin thời tiết hiện tại của nông trại dựa trên tọa độ địa chỉ nông trại.")]
    public async Task<ActionResult<APIResponse>> GetCurrentWeatherDetailsByFarmId(ulong farmId)
    {
        try
        {
            var weatherData = await _weatherService.GetCurrentWeatherDetailsByFarmIdAsync(farmId, GetCancellationToken());
            return SuccessResponse(weatherData, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}