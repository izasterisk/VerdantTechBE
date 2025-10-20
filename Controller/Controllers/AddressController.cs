using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class AddressController : BaseController
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>
    /// Lấy danh sách tất cả tỉnh/thành phố từ GoShip
    /// </summary>
    /// <returns>Danh sách tỉnh/thành phố</returns>
    [HttpGet("provinces")]
    [Authorize]
    [EndpointSummary("Get All Provinces")]
    public async Task<ActionResult<APIResponse>> GetProvinces()
    {
        try
        {
            var provinces = await _addressService.GetProvincesAsync(GetCancellationToken());
            return SuccessResponse(provinces);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách quận/huyện theo tỉnh/thành phố từ GoShip
    /// </summary>
    /// <param name="provinceId">ID của tỉnh/thành phố (string)</param>
    /// <returns>Danh sách quận/huyện</returns>
    [HttpGet("districts")]
    [Authorize]
    [EndpointSummary("Get Districts By Province")]
    public async Task<ActionResult<APIResponse>> GetDistricts([FromQuery] string provinceId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(provinceId))
                return ErrorResponse("Province ID không được để trống");

            var districts = await _addressService.GetDistrictsAsync(provinceId, GetCancellationToken());
            return SuccessResponse(districts);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách phường/xã theo quận/huyện từ GoShip
    /// </summary>
    /// <param name="districtId">ID của quận/huyện (string)</param>
    /// <returns>Danh sách phường/xã</returns>
    [HttpGet("communes")]
    [Authorize]
    [EndpointSummary("Get Communes By District")]
    public async Task<ActionResult<APIResponse>> GetCommunes([FromQuery] string districtId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(districtId))
                return ErrorResponse("District ID không được để trống");

            var communes = await _addressService.GetCommunesAsync(districtId, GetCancellationToken());
            return SuccessResponse(communes);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}