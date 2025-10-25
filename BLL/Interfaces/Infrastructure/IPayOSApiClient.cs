using Net.payOS.Types;

namespace BLL.Interfaces.Infrastructure;

public interface IPayOSApiClient
{
    /// <summary>
    /// Tạo payment link trên PayOS
    /// </summary>
    Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData);

    /// <summary>
    /// Lấy thông tin payment link từ PayOS
    /// </summary>
    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode);

    /// <summary>
    /// Hủy payment link trên PayOS
    /// </summary>
    Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string? reason = null);

    /// <summary>
    /// Verify webhook data từ PayOS
    /// </summary>
    WebhookData VerifyWebhookData(WebhookType webhookBody);

    /// <summary>
    /// Confirm webhook URL với PayOS
    /// </summary>
    Task ConfirmWebhookAsync(string webhookUrl);
}