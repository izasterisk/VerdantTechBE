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
                       ?? throw new InvalidOperationException("Không tìm thấy PAYOS_CLIENT_ID trong biến môi trường");
        var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") 
                     ?? throw new InvalidOperationException("Không tìm thấy PAYOS_API_KEY trong biến môi trường");
        var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") 
                          ?? throw new InvalidOperationException("Không tìm thấy PAYOS_CHECKSUM_KEY trong biến môi trường");
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
            throw new InvalidOperationException($"Không thể lấy danh sách ngân hàng từ VietQR API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể phân tích dữ liệu ngân hàng: {ex.Message}", ex);
        }
    }

    public async Task<(string IPv4, string IPv6)> GetIPAddressAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ipv4Url = "https://api.ipify.org?format=json";
            var ipv6Url = "https://api64.ipify.org?format=json";
            
            // Call both APIs in parallel
            var ipv4Task = _httpClient.GetAsync(ipv4Url, cancellationToken);
            var ipv6Task = _httpClient.GetAsync(ipv6Url, cancellationToken);
            
            await Task.WhenAll(ipv4Task, ipv6Task);
            
            var ipv4Response = await ipv4Task;
            var ipv6Response = await ipv6Task;
            
            ipv4Response.EnsureSuccessStatusCode();
            ipv6Response.EnsureSuccessStatusCode();
            
            var ipv4Content = await ipv4Response.Content.ReadAsStringAsync(cancellationToken);
            var ipv6Content = await ipv6Response.Content.ReadAsStringAsync(cancellationToken);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            using var ipv4Doc = JsonDocument.Parse(ipv4Content);
            using var ipv6Doc = JsonDocument.Parse(ipv6Content);
            
            var ipv4 = ipv4Doc.RootElement.GetProperty("ip").GetString() ?? string.Empty;
            var ipv6 = ipv6Doc.RootElement.GetProperty("ip").GetString() ?? string.Empty;
            
            return (ipv4, ipv6);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể lấy địa chỉ IP từ API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể phân tích dữ liệu địa chỉ IP: {ex.Message}", ex);
        }
    }
}