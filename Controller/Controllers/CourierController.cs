using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Courier;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CourierController : BaseController
{
    private readonly ICourierService _courierService;
    
    public CourierController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>
    /// Lấy danh sách tất cả tỉnh/thành phố
    /// </summary>
    /// <returns>Danh sách tỉnh/thành phố</returns>
    [HttpGet("cities")]
    [EndpointSummary("Get All Cities")]
    [EndpointDescription("Lấy danh sách tất cả tỉnh/thành phố từ GoShip API.")]
    public async Task<ActionResult<APIResponse>> GetCities()
    {
        try
        {
            var cities = await _courierService.GetCitiesAsync(GetCancellationToken());
            return SuccessResponse(cities, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách quận/huyện theo mã tỉnh/thành phố
    /// </summary>
    /// <param name="cityId">Mã tỉnh/thành phố (6 chữ số)</param>
    /// <returns>Danh sách quận/huyện</returns>
    [HttpGet("districts/{cityId}")]
    [EndpointSummary("Get Districts By City ID")]
    [EndpointDescription("Lấy danh sách quận/huyện theo mã tỉnh/thành phố từ GoShip API.")]
    public async Task<ActionResult<APIResponse>> GetDistricts(string cityId)
    {
        try
        {
            var districts = await _courierService.GetDistrictsByCityIdAsync(cityId, GetCancellationToken());
            return SuccessResponse(districts, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách phường/xã theo mã quận/huyện
    /// </summary>
    /// <param name="districtId">Mã quận/huyện (6 chữ số)</param>
    /// <returns>Danh sách phường/xã</returns>
    [HttpGet("wards/{districtId}")]
    [EndpointSummary("Get Wards By District ID")]
    [EndpointDescription("Lấy danh sách phường/xã theo mã quận/huyện từ GoShip API.")]
    public async Task<ActionResult<APIResponse>> GetWards(string districtId)
    {
        try
        {
            var wards = await _courierService.GetWardsByDistrictIdAsync(districtId, GetCancellationToken());
            return SuccessResponse(wards, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}