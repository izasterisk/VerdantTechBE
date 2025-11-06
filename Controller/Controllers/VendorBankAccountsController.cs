using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL.Interfaces;
using BLL.Interfaces.Infrastructure;
using BLL.DTO;
using BLL.DTO.VendorBankAccount;
using System.Net;

namespace Controller.Controllers;

[Route("api/[controller]")]
public class VendorBankAccountsController : BaseController
{
    private readonly IVendorBankAccountsService _vendorBankAccountsService;
    private readonly IPayOSApiClient _payOSApiClient;

    public VendorBankAccountsController(
        IVendorBankAccountsService vendorBankAccountsService,
        IPayOSApiClient payOSApiClient)
    {
        _vendorBankAccountsService = vendorBankAccountsService;
        _payOSApiClient = payOSApiClient;
    }

    /// <summary>
    /// Tạo tài khoản ngân hàng mới cho vendor
    /// </summary>
    /// <param name="userId">ID của vendor</param>
    /// <param name="dto">Thông tin tài khoản ngân hàng cần tạo</param>
    /// <returns>Thông tin tài khoản ngân hàng đã tạo</returns>
    [HttpPost("vendor/{userId}")]
    [Authorize(Roles = "Vendor,Admin")]
    [EndpointSummary("Create Vendor Bank Account")]
    [EndpointDescription("Tạo tài khoản ngân hàng mới cho vendor. Chỉ Vendor (chủ tài khoản) và Admin mới có quyền thực hiện.")]
    public async Task<ActionResult<APIResponse>> CreateVendorBankAccount(ulong userId, [FromBody] VendorBankAccountCreateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var account = await _vendorBankAccountsService.CreateVendorBankAccountAsync(userId, dto, GetCancellationToken());
            return SuccessResponse(account, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Cập nhật tài khoản ngân hàng của vendor
    /// </summary>
    /// <param name="accountId">ID của tài khoản ngân hàng</param>
    /// <param name="dto">Thông tin tài khoản ngân hàng cần cập nhật</param>
    /// <returns>Thông tin tài khoản ngân hàng đã cập nhật</returns>
    [HttpPatch("{accountId}")]
    [Authorize(Roles = "Vendor,Admin")]
    [EndpointSummary("Update Vendor Bank Account")]
    public async Task<ActionResult<APIResponse>> UpdateVendorBankAccount(ulong accountId, [FromBody] VendorBankAccountUpdateDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var account = await _vendorBankAccountsService.UpdateVendorBankAccountAsync(accountId, dto, GetCancellationToken());
            return SuccessResponse(account);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Lấy danh sách tất cả tài khoản ngân hàng của vendor
    /// </summary>
    /// <param name="vendorId">ID của vendor</param>
    /// <returns>Danh sách tài khoản ngân hàng</returns>
    [HttpGet("vendor/{vendorId}")]
    [Authorize(Roles = "Vendor,Admin,Staff")]
    [EndpointSummary("Get All Vendor Bank Accounts")]
    [EndpointDescription("Lấy danh sách tất cả tài khoản ngân hàng của vendor theo ID. Vendor, Admin và Staff có quyền truy cập.")]
    public async Task<ActionResult<APIResponse>> GetAllVendorBankAccountsByVendorId(ulong vendorId)
    {
        try
        {
            var accounts = await _vendorBankAccountsService.GetAllVendorBankAccountsByVendorIdAsync(vendorId, GetCancellationToken());
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