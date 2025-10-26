using BLL.DTO.Payment.PayOS;
using Net.payOS.Types;

namespace BLL.Interfaces;

public interface IPayOSService
{
    Task<CreatePaymentResult> CreatePaymentLinkAsync(ulong orderId, CreatePaymentDataDTO dto);
    Task<WebhookData> HandlePayOSWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default);
    Task ConfirmWebhookAsync(ConfirmWebhookDTO dto, CancellationToken cancellationToken = default);
}