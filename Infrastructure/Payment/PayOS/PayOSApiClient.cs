using System.Text.Json;
using BLL.DTO.Payment;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Payment.PayOS.Models;
using Net.payOS.Types;

namespace Infrastructure.Payment.PayOS;

public class PayOSApiClient : IPayOSApiClient
{
    private readonly Net.payOS.PayOS _payOS;
    private readonly HttpClient _httpClient;
    
    public PayOSApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    public async Task<List<BankInfoDTO>> GetAllSupportedBanksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "https://api.vietqr.io/v2/banks";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            // Parse JSON manually to access nested "data" array
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var dataArray = jsonDoc.RootElement.GetProperty("data");
            
            var banks = JsonSerializer.Deserialize<List<BankInfo>>(dataArray.GetRawText(), options);
            
            if (banks == null)
                return new List<BankInfoDTO>();
            
            // Filter banks: transferSupported = 1, lookupSupported = 1, isTransfer = 1, support != 0
            var filteredBanks = banks
                .Where(b => b.TransferSupported == 1 
                         && b.LookupSupported == 1 
                         && b.IsTransfer == 1 
                         && b.Support != 0)
                .Select(b => new BankInfoDTO
                {
                    Name = b.Name,
                    Code = b.Code,
                    Bin = b.Bin,
                    ShortName = b.ShortName,
                    Logo = b.Logo
                })
                .ToList();
            
            return filteredBanks;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to get banks from VietQR API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse bank response: {ex.Message}", ex);
        }
    }
}