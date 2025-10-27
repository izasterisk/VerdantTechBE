using Microsoft.AspNetCore.Mvc;
using BLL.Interfaces;
using BLL.DTO;
using BLL.DTO.Payment.PayOS;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Net.payOS.Types;

namespace Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayOSController : BaseController
{
    private readonly IPayOSService _payOSService;
    
    public PayOSController(IPayOSService payOSService)
    {
        _payOSService = payOSService;
    }

    /// <summary>
    /// Tạo payment link cho đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="dto">Thông tin thanh toán</param>
    /// <returns>Thông tin payment link</returns>
    [HttpPost("create/{orderId}")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> CreatePaymentLink(ulong orderId, [FromBody] CreatePaymentDataDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var result = await _payOSService.CreatePaymentLinkAsync(orderId, dto);
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Webhook handler cho PayOS - Nhận thông báo thanh toán từ PayOS
    /// </summary>
    /// <param name="webhookBody">Dữ liệu webhook từ PayOS</param>
    /// <returns>Response xác nhận đã nhận webhook</returns>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<APIResponse>> PayOSWebhookHandler([FromBody] WebhookType webhookBody)
    {
        try
        {
            var webhookData = await _payOSService.HandlePayOSWebhookAsync(webhookBody, GetCancellationToken());
            return Ok(APIResponse.Success(new { message = "Webhook đã xử lý thành công.", data = webhookData }));
        }
        catch (Exception ex)
        {
            // IMPORTANT: Always return 200 OK to PayOS, even on error
            // PayOS requires 200 response to consider webhook delivered
            return Ok(APIResponse.Error($"Webhook error: {ex.Message}", HttpStatusCode.OK));
        }
    }

    /// <summary>
    /// Xác nhận webhook URL với PayOS
    /// </summary>
    /// <param name="dto">DTO chứa webhook URL</param>
    /// <returns>Response xác nhận</returns>
    [HttpPost("confirm-webhook")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> ConfirmWebhook([FromBody] ConfirmWebhookDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;
        try
        {
            await _payOSService.ConfirmWebhookAsync(dto, GetCancellationToken());
            return SuccessResponse(new { message = "Webhook đã xác nhận thành công." });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}