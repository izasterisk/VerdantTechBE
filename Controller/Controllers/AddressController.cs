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
    /// Lấy danh sách tất cả tỉnh/thành phố từ GHN
    /// </summary>
    /// <returns>Danh sách tỉnh/thành phố</returns>
    [HttpGet("provinces")]
    [AllowAnonymous]
    [EndpointSummary("Get All Provinces")]
    [EndpointDescription("Lấy danh sách tất cả tỉnh/thành phố từ GHN API. Không yêu cầu authentication.")]
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
    /// Lấy danh sách quận/huyện từ GHN (có thể filter theo tỉnh/thành phố)
    /// </summary>
    /// <param name="provinceId">ID của tỉnh/thành phố để filter (optional)</param>
    /// <returns>Danh sách quận/huyện</returns>
    [HttpGet("districts")]
    [AllowAnonymous]
    [EndpointSummary("Get Districts (with optional Province filter)")]
    [EndpointDescription("Lấy danh sách quận/huyện từ GHN API. Không yêu cầu authentication. " +
                         "Có thể filter theo provinceId. Ví dụ: /api/Address/districts?provinceId=202")]
    public async Task<ActionResult<APIResponse>> GetDistricts([FromQuery] int? provinceId = null)
    {
        try
        {
            var districts = await _addressService.GetDistrictsAsync(provinceId, GetCancellationToken());
            return SuccessResponse(districts);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách phường/xã theo quận/huyện từ GHN
    /// </summary>
    /// <param name="districtId">ID của quận/huyện</param>
    /// <returns>Danh sách phường/xã</returns>
    [HttpGet("communes")]
    [AllowAnonymous]
    [EndpointSummary("Get Communes By District")]
    [EndpointDescription("Lấy danh sách phường/xã theo ID quận/huyện từ GHN API. Không yêu cầu authentication. " +
                         "Ví dụ: /api/Address/communes?districtId=1735")]
    public async Task<ActionResult<APIResponse>> GetCommunes([FromQuery] int districtId)
    {
        try
        {
            if (districtId <= 0)
                return ErrorResponse("District ID phải lớn hơn 0", HttpStatusCode.BadRequest);

            var communes = await _addressService.GetCommunesAsync(districtId, GetCancellationToken());
            return SuccessResponse(communes);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}