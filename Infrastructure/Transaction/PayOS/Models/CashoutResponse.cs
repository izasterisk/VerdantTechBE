using System.Text.Json.Serialization;

namespace Infrastructure.Payment.PayOS.Models;

public class CashoutResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = null!;
    
    [JsonPropertyName("desc")]
    public string Desc { get; set; } = null!;
    
    [JsonPropertyName("data")]
    public CashoutData? Data { get; set; }
}

public class CashoutData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; set; } = null!;
    
    [JsonPropertyName("transactions")]
    public List<CashoutTransaction> Transactions { get; set; } = new();
    
    [JsonPropertyName("category")]
    public List<string> Category { get; set; } = new();
    
    [JsonPropertyName("approvalState")]
    public string ApprovalState { get; set; } = null!;
    
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = null!;
}

public class CashoutTransaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;
    
    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; set; } = null!;
    
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = null!;
    
    [JsonPropertyName("toBin")]
    public string ToBin { get; set; } = null!;
    
    [JsonPropertyName("toAccountNumber")]
    public string ToAccountNumber { get; set; } = null!;
    
    [JsonPropertyName("toAccountName")]
    public string ToAccountName { get; set; } = null!;
    
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }
    
    [JsonPropertyName("transactionDatetime")]
    public string? TransactionDatetime { get; set; }
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }
    
    [JsonPropertyName("state")]
    public string State { get; set; } = null!;
}
