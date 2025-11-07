using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class WalletController : BaseController
{
    private readonly IWalletService _walletService;
    
    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Xử lý cộng tiền vào ví vendor từ các đơn hàng đã giao quá 7 ngày
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <returns>Thông tin ví sau khi cập nhật</returns>
    [HttpPost("{userId}/process-credits")]
    [Authorize(Roles = "Admin,Staff,Vendor")]
    [EndpointSummary("Process Wallet Credits")]
    [EndpointDescription("Trả về số dư trong wallet của vendor.")]
    public async Task<ActionResult<APIResponse>> ProcessWalletCredits(ulong userId)
    {
        try
        {
            var wallet = await _walletService.ProcessWalletCreditsAsync(userId, GetCancellationToken());
            return SuccessResponse(wallet);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
