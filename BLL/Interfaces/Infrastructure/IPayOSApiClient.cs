using Net.payOS.Types;

namespace BLL.Interfaces.Infrastructure;

public interface IPayOSApiClient
{
    Task<CreatePaymentResult> CreatePaymentLinkAsync(
        long orderCode, 
        int amount, 
        string description, 
        List<ItemData> items, 
        string cancelUrl, 
        string returnUrl);

    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode);

    Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string? reason = null);

    WebhookData VerifyWebhookData(WebhookType webhookBody);

    Task ConfirmWebhookAsync(string webhookUrl);
}