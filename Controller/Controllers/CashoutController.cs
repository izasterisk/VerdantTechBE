using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class CashoutController : BaseController
{
    private readonly ICashoutService _cashoutService;
    
    public CashoutController(ICashoutService cashoutService)
    {
        _cashoutService = cashoutService;
    }

    /// <summary>
    /// Lấy địa chỉ IP (IPv4 và IPv6)
    /// </summary>
    /// <returns>Thông tin địa chỉ IP</returns>
    [HttpGet("ip-address")]
    [Authorize]
    [EndpointSummary("Get IP Address")]
    [EndpointDescription("Lấy địa chỉ IP hiện tại (IPv4 và IPv6). Endpoint này chỉ sử dụng cho BE, FE không cần làm.")]
    public async Task<ActionResult<APIResponse>> GetIPAddress()
    {
        try
        {
            var (ipv4, ipv6) = await _cashoutService.GetIPAddressAsync(GetCancellationToken());
            var result = new { IPv4 = ipv4, IPv6 = ipv6 };
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy số dư tài khoản PayOS Payout
    /// </summary>
    /// <returns>Số dư tài khoản</returns>
    [HttpGet("balance")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get PayOS Payout Balance")]
    [EndpointDescription("Lấy số dư tài khoản PayOS Payout để xử lý cashout.")]
    public async Task<ActionResult<APIResponse>> GetBalance()
    {
        try
        {
            var balance = await _cashoutService.GetBalanceAsync(GetCancellationToken());
            var result = new { Balance = balance };
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}