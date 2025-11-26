using BLL.DTO.Payment.PayOS;
using BLL.DTO.Transaction;
using BLL.Services.Payment;
using Net.payOS.Types;

namespace BLL.Interfaces;

public interface IPayOSService
{
    Task<CreatePaymentResult> CreatePaymentLinkAsync(ulong orderId, CreatePaymentDataDTO dto, CancellationToken cancellationToken = default);
    Task<WebhookData> HandlePayOSWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default);
    Task ConfirmWebhookAsync(ConfirmWebhookDTO dto, CancellationToken cancellationToken = default);
    Task<TransactionResponseDTO> GetPaymentLinkInformationAsync(ulong transactionId, CancellationToken cancellationToken = default);
}