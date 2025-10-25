using BLL.Interfaces.Infrastructure;
using Net.payOS.Types;

namespace Infrastructure.Payment.PayOS;

public class PayOSApiClient : IPayOSApiClient
{
    private readonly Net.payOS.PayOS _payOS;
    
    public PayOSApiClient()
    {
        var clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") 
                       ?? throw new InvalidOperationException("PAYOS_CLIENT_ID not found in environment variables");
        var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") 
                     ?? throw new InvalidOperationException("PAYOS_API_KEY not found in environment variables");
        var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") 
                          ?? throw new InvalidOperationException("PAYOS_CHECKSUM_KEY not found in environment variables");
        _payOS = new Net.payOS.PayOS(clientId, apiKey, checksumKey);
    }
    
    public async Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData)
    {
        return await _payOS.createPaymentLink(paymentData);
    }

    public async Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode)
    {
        return await _payOS.getPaymentLinkInformation(orderCode);
    }

    public async Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string? reason = null)
    {
        return await _payOS.cancelPaymentLink(orderCode, reason);
    }

    public WebhookData VerifyWebhookData(WebhookType webhookBody)
    {
        return _payOS.verifyPaymentWebhookData(webhookBody);
    }

    public async Task ConfirmWebhookAsync(string webhookUrl)
    {
        await _payOS.confirmWebhook(webhookUrl);
    }
}