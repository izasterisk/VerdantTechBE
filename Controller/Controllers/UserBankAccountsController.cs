using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.DTO;
using BLL.DTO.UserBankAccount;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class UserBankAccountsController : BaseController
{
    private readonly IUserBankAccountsService _userBankAccountsService;
    private readonly IPayOSApiClient _payOSApiClient;

    public UserBankAccountsController(
        IUserBankAccountsService userBankAccountsService,
        IPayOSApiClient payOSApiClient)
    {
        _userBankAccountsService = userBankAccountsService;
        _payOSApiClient = payOSApiClient;
    }

    /// <summary>
    /// Tạo tài khoản ngân hàng mới cho người dùng
    /// </summary>
    /// <param name="userId">ID của người dùng</param>
    /// <param name="dto">Thông tin tài khoản ngân hàng cần tạo</param>
    /// <returns>Thông tin tài khoản ngân hàng đã tạo</returns>
    [HttpPost("user/{userId}")]
    [Authorize]
    [EndpointSummary("Create User Bank Account")]
    public async Task<ActionResult<APIResponse>> CreateUserBankAccount(ulong userId, [FromBody] UserBankAccountCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var account = await _userBankAccountsService.CreateUserBankAccountAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(account, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Vô hiệu hóa tài khoản ngân hàng của người dùng (soft delete)
    /// </summary>
    /// <param name="accountId">ID của tài khoản ngân hàng</param>
    /// <returns>Kết quả vô hiệu hóa</returns>
    [HttpPatch("{accountId}/deactivate")]
    [Authorize(Roles = "Staff,Vendor,Admin")]
    [EndpointSummary("Soft Delete User Bank Account")]
    [EndpointDescription("Vô hiệu hóa tài khoản ngân hàng của người dùng (không xóa tài khoản hoàn toàn khỏi DB).")]
    public async Task<ActionResult<APIResponse>> DeleteUserBankAccount(ulong accountId)
    {
        try
        {
            var result = await _userBankAccountsService.SoftDeleteUserBankAccountAsync(accountId, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả tài khoản ngân hàng của người dùng
    /// </summary>
    /// <param name="userId">ID của người dùng</param>
    /// <returns>Danh sách tài khoản ngân hàng</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Customer,Vendor,Admin,Staff")]
    [EndpointSummary("Get All User Bank Accounts")]
    [EndpointDescription("Lấy danh sách tất cả tài khoản ngân hàng của người dùng theo ID. Người dùng, Admin và Staff có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetAllUserBankAccountsByUserId(ulong userId)
    {
        try
        {
            var accounts = await _userBankAccountsService.GetAllUserBankAccountsByUserIdAsync(userId, GetCancellationToken());
            return SuccessResponse(accounts);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách các ngân hàng được hỗ trợ từ VietQR
    /// </summary>
    /// <returns>Danh sách ngân hàng được hỗ trợ (đã lọc theo điều kiện: transferSupported=1, lookupSupported=1, isTransfer=1, support≠0)</returns>
    [HttpGet("supported-banks")]
    [EndpointSummary("Get All Supported Banks")]
    public async Task<ActionResult<APIResponse>> GetAllSupportedBanks()
    {
        try
        {
            var banks = await _payOSApiClient.GetAllSupportedBanksAsync(GetCancellationToken());
            return SuccessResponse(banks);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
