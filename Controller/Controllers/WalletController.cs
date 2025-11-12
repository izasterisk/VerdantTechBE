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
    /// Lấy danh sách tất cả yêu cầu rút tiền với phân trang
    /// </summary>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách yêu cầu rút tiền có phân trang</returns>
    [HttpGet("cashout-requests")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Get All Cashout Requests")]
    [EndpointDescription("Lấy danh sách tất cả yêu cầu rút tiền đang pending với phân trang. Chỉ Admin/Staff mới có quyền. Mẫu: /api/Wallet/cashout-requests?page=1&pageSize=10")]
    public async Task<ActionResult<APIResponse>> GetAllCashoutRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var cashoutRequests = await _walletService.GetAllWalletCashoutRequestAsync(page, pageSize, GetCancellationToken());
            return SuccessResponse(cashoutRequests);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách yêu cầu rút tiền của một vendor cụ thể với phân trang
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <param name="page">Số trang (mặc định: 1)</param>
    /// <param name="pageSize">Số bản ghi mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách yêu cầu rút tiền của vendor có phân trang</returns>
    [HttpGet("{userId}/cashout-requests")]
    [Authorize(Roles = "Admin,Staff,Vendor")]
    [EndpointSummary("Get All Cashout Requests By User")]
    [EndpointDescription("Lấy danh sách tất cả yêu cầu rút tiền của một vendor cụ thể với phân trang. Mẫu: /api/Wallet/{userId}/cashout-requests?page=1&pageSize=10")]
    public async Task<ActionResult<APIResponse>> GetAllCashoutRequestsByUserId(ulong userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                return ErrorResponse("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 100)
                return ErrorResponse("Page size must be between 1 and 100");

            var cashoutRequests = await _walletService.GetAllWalletCashoutRequestByUserIdAsync(userId, page, pageSize, GetCancellationToken());
            return SuccessResponse(cashoutRequests);
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

    /// <summary>
    /// Xử lý yêu cầu rút tiền thủ công
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <param name="dto">Thông tin xử lý (status, gatewayPaymentId)</param>
    /// <returns>Thông tin cashout đã được xử lý</returns>
    [HttpPost("{userId}/process-cashout-manual")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Process Cashout Request Manually")]
    [EndpointDescription("Xử lý yêu cầu rút tiền của vendor thủ công. Chỉ Admin/Staff mới có quyền. Status có thể là: Completed, Failed, Cancelled. " +
                         "Completed bắt buộc có gatewayPaymentId, Failed/Cancelled bắt buộc có CancelReason.")]
    public async Task<ActionResult<APIResponse>> ProcessCashoutRequestManual(ulong userId, [FromBody] WalletProcessCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var staffId = GetCurrentUserId();
            var cashoutResponse = await _walletService.ProcessWalletCashoutRequestAsync(staffId, userId, dto, GetCancellationToken());
            return SuccessResponse(cashoutResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Xử lý yêu cầu rút tiền qua PayOS
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <returns>Thông tin cashout đã được xử lý</returns>
    [HttpPost("{userId}/process-cashout")]
    [Authorize(Roles = "Admin,Staff")]
    [EndpointSummary("Process Cashout Request via PayOS")]
    [EndpointDescription("Xử lý yêu cầu rút tiền của vendor tự động qua PayOS. Chỉ Admin/Staff mới có quyền.")]
    public async Task<ActionResult<APIResponse>> ProcessCashoutRequest(ulong userId)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var cashoutResponse = await _walletService.ProcessWalletCashoutRequestByPayOSAsync(staffId, userId, GetCancellationToken());
            return SuccessResponse(cashoutResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
