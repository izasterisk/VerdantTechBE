using System.Text.Json.Serialization;

namespace Infrastructure.Payment.PayOS.Models;

public class BankInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("bin")]
    public string Bin { get; set; } = string.Empty;

    [JsonPropertyName("shortName")]
    public string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("logo")]
    public string Logo { get; set; } = string.Empty;

    [JsonPropertyName("transferSupported")]
    public int TransferSupported { get; set; }

    [JsonPropertyName("lookupSupported")]
    public int LookupSupported { get; set; }

    [JsonPropertyName("short_name")]
    public string Short_Name { get; set; } = string.Empty;

    [JsonPropertyName("support")]
    public int Support { get; set; }

    [JsonPropertyName("isTransfer")]
    public int IsTransfer { get; set; }

    [JsonPropertyName("swift_code")]
    public string? SwiftCode { get; set; }
}
