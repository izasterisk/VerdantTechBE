using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Wallet;
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
    [EndpointSummary("Get Wallet Credits")]
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

    /// <summary>
    /// Tạo yêu cầu rút tiền từ ví
    /// </summary>
    /// <param name="dto">Thông tin yêu cầu rút tiền</param>
    /// <returns>Thông tin yêu cầu rút tiền đã tạo</returns>
    [HttpPost("cashout-request")]
    [Authorize(Roles = "Vendor")]
    [EndpointSummary("Create Cashout Request")]
    [EndpointDescription("Tạo yêu cầu rút tiền từ ví. Vendor chỉ được tạo 1 yêu cầu pending tại một thời điểm.")]
    public async Task<ActionResult<APIResponse>> CreateCashoutRequest([FromBody] WalletCashoutRequestCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var userId = GetCurrentUserId();
            var cashoutRequest = await _walletService.CreateWalletCashoutRequestAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(cashoutRequest, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy thông tin yêu cầu rút tiền đang pending
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <returns>Thông tin yêu cầu rút tiền</returns>
    [HttpGet("{userId}/cashout-request")]
    [Authorize(Roles = "Admin,Staff,Vendor")]
    [EndpointSummary("Get Pending Cashout Request")]
    [EndpointDescription("Lấy thông tin yêu cầu rút tiền đang pending của vendor.")]
    public async Task<ActionResult<APIResponse>> GetCashoutRequest(ulong userId)
    {
        try
        {
            var cashoutRequest = await _walletService.GetWalletCashoutRequestAsync(userId, GetCancellationToken());
            return SuccessResponse(cashoutRequest);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xóa yêu cầu rút tiền đang pending
    /// </summary>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("cashout-request")]
    [Authorize(Roles = "Vendor")]
    [EndpointSummary("Delete Pending Cashout Request")]
    [EndpointDescription("Xóa yêu cầu rút tiền đang pending. Chỉ vendor sở hữu yêu cầu mới có thể xóa.")]
    public async Task<ActionResult<APIResponse>> DeleteCashoutRequest()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _walletService.DeleteWalletCashoutRequestAsync(userId, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
