using System.Text.Json;
using BLL.DTO.Cashout;
using BLL.DTO.Payment;
using BLL.DTO.UserBankAccount;
using BLL.Interfaces.Infrastructure;
using Infrastructure.Payment.PayOS.Models;
using Net.payOS.Types;

namespace Infrastructure.Payment.PayOS;

public class PayOSApiClient : IPayOSApiClient
{
    private readonly Net.payOS.PayOS _payOS;
    private readonly HttpClient _httpClient;
    private readonly string _merchantHost = "https://api-merchant.payos.vn";
    
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

    public async Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_merchantHost}/v1/payouts-account/balance";
            
            var clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID_PAYOUT") 
                           ?? throw new InvalidOperationException("Không tìm thấy PAYOS_CLIENT_ID_PAYOUT trong biến môi trường");
            var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY_PAYOUT") 
                         ?? throw new InvalidOperationException("Không tìm thấy PAYOS_API_KEY_PAYOUT trong biến môi trường");
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-client-id", clientId);
            request.Headers.Add("x-api-key", apiKey);
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            
            var code = root.GetProperty("code").GetString();
            var desc = root.GetProperty("desc").GetString();
            
            if (code != "00")
            {
                throw new InvalidOperationException(desc ?? "Không thể lấy thông tin số dư từ PayOS");
            }
            
            var data = root.GetProperty("data");
            var balanceString = data.GetProperty("balance").GetString() ?? "0";
            
            if (!decimal.TryParse(balanceString, out var balance))
            {
                throw new InvalidOperationException("Không thể phân tích giá trị số dư");
            }
            
            return balance;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối tới PayOS API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể phân tích dữ liệu số dư: {ex.Message}", ex);
        }
    }

    public async Task<PayOSCashoutResponseDTO> CreateCashoutAsync(UserBankAccountResponseDTO bankAccount, int amount, 
        string description, List<string> categories, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_merchantHost}/v1/payouts";
            
            var clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID_PAYOUT") 
                           ?? throw new InvalidOperationException("Không tìm thấy PAYOS_CLIENT_ID_PAYOUT trong biến môi trường");
            var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY_PAYOUT") 
                         ?? throw new InvalidOperationException("Không tìm thấy PAYOS_API_KEY_PAYOUT trong biến môi trường");
            var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY_PAYOUT") 
                              ?? throw new InvalidOperationException("Không tìm thấy PAYOS_CHECKSUM_KEY_PAYOUT trong biến môi trường");
            
            // Generate reference ID and idempotency key
            var referenceId = PayOSApiClientHelpers.GenerateReferenceId();
            var idempotencyKey = PayOSApiClientHelpers.GenerateIdempotencyKey();
            
            // Create request body
            var requestBody = new Dictionary<string, object?>
            {
                { "referenceId", referenceId },
                { "amount", amount },
                { "description", description },
                { "toBin", bankAccount.BankCode },
                { "toAccountNumber", bankAccount.AccountNumber },
                { "category", categories }
            };
            
            // Create signature
            var signature = PayOSApiClientHelpers.CreateSignature(checksumKey, requestBody);
            
            // Create HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-client-id", clientId);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("x-signature", signature);
            request.Headers.Add("x-idempotency-key", idempotencyKey);
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            // Read response content BEFORE checking status code to preserve error details
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            var options2 = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var cashoutResponse = JsonSerializer.Deserialize<CashoutResponse>(responseContent, options2);
            
            if (cashoutResponse?.Code != "00")
            {
                var errorDesc = cashoutResponse?.Desc ?? "Lỗi không xác định";
                throw new InvalidOperationException(errorDesc);
            }
            
            if (cashoutResponse.Data == null || cashoutResponse.Data.Transactions == null || !cashoutResponse.Data.Transactions.Any())
            {
                throw new InvalidOperationException("Không có giao dịch nào được tạo");
            }
            
            // Map first transaction to DTO (use batch ID instead of transaction ID)
            var transaction = cashoutResponse.Data.Transactions.First();
            
            return new PayOSCashoutResponseDTO
            {
                Id = cashoutResponse.Data.Id, // Use batch ID from Data
                ReferenceId = transaction.ReferenceId,
                Amount = transaction.Amount,
                Description = transaction.Description,
                ToBin = transaction.ToBin,
                ToAccountNumber = transaction.ToAccountNumber,
                ToAccountName = transaction.ToAccountName,
                Reference = transaction.Reference,
                TransactionDatetime = transaction.TransactionDatetime,
                ErrorMessage = transaction.ErrorMessage,
                ErrorCode = transaction.ErrorCode,
                State = transaction.State
            };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Không thể kết nối tới PayOS API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Không thể phân tích dữ liệu cashout: {ex.Message}", ex);
        }
    }
}