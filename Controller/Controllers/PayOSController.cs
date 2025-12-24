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
    /// Tạo payment link PayOS cho đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng cần thanh toán</param>
    /// <param name="dto">Thông tin thanh toán</param>
    /// <returns>Thông tin payment link bao gồm checkoutUrl, orderCode, paymentLinkId</returns>
    [HttpPost("create/{orderId}")]
    [EndpointDescription("Chỉ dành cho thanh toán đơn hàng thông thường. " +
                         "Đơn hàng phải ở trạng thái Pending và không phải COD. " +
                         "Description không được là \"12MONTHS\" hoặc \"6MONTHS\" (dùng CreateSubscriptionLink thay thế).")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> CreatePaymentLink(ulong orderId, [FromBody] CreatePaymentDataDTO dto)
    {
        var validationResult = ValidateModel();
        if (validationResult != null) return validationResult;

        try
        {
            var result = await _payOSService.CreatePaymentLinkAsync(orderId, dto, GetCancellationToken());
            return SuccessResponse(result, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Tạo payment link PayOS cho gói đăng ký thương nhân (Vendor Subscription)
    /// </summary>
    /// <param name="vendorUserId">User ID của vendor cần đăng ký</param>
    /// <param name="type">Loại gói đăng ký: "12MONTHS" hoặc "6MONTHS"</param>
    /// <param name="price">Giá gói đăng ký (VND)</param>
    /// <returns>Thông tin payment link bao gồm checkoutUrl</returns>
    [HttpPost("create-subscription")]
    [EndpointDescription("Vendor không được có gói đăng ký đang hoạt động. " +
                         "Type chỉ chấp nhận: \"12MONTHS\" (12 tháng) hoặc \"6MONTHS\" (6 tháng). " +
                         "Sau khi thanh toán thành công, hệ thống sẽ tự động kích hoạt subscription cho vendor.")]
    [Authorize]
    public async Task<ActionResult<APIResponse>> CreateSubscriptionLink(ulong vendorUserId, string type, int price)
    {
        try
        {
            var result = await _payOSService.CreateSubscriptionLinkAsync(vendorUserId, type, price, GetCancellationToken());
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

    /// <summary>
    /// Lấy thông tin payment link theo transaction ID
    /// </summary>
    /// <param name="transactionId">ID của giao dịch</param>
    /// <returns>Thông tin payment link</returns>
    [HttpGet("payment-info/{transactionId}")]
    [Authorize]
    [EndpointDescription("Lấy thông tin chi tiết của một giao dịch thanh toán.")]
    public async Task<ActionResult<APIResponse>> GetPaymentLinkInformation(ulong transactionId)
    {
        try
        {
            var result = await _payOSService.GetPaymentLinkInformationAsync(transactionId, GetCancellationToken());
            return SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}