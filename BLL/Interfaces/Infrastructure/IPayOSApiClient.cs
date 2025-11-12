using BLL.DTO.Cashout;
using BLL.DTO.Payment;
using BLL.DTO.UserBankAccount;
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

    /// <summary>
    /// Lấy danh sách tất cả các ngân hàng được hỗ trợ từ VietQR API
    /// </summary>
    Task<List<BankInfoDTO>> GetAllSupportedBanksAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy địa chỉ IP hiện tại (IPv4 và IPv6)
    /// </summary>
    Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy số dư tài khoản PayOS Payout
    /// </summary>
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tạo lệnh rút tiền (cashout) qua PayOS
    /// </summary>
    Task<PayOSCashoutResponseDTO> CreateCashoutAsync(
        UserBankAccountResponseDTO bankAccount, 
        int amount, 
        string description,
        List<string> categories,
        CancellationToken cancellationToken = default);
}