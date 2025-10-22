using System.Text.Json.Serialization;

namespace Infrastructure.Courier.Models;

public class RateResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_name")]
    public string CarrierName { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_logo")]
    public string? CarrierLogo { get; set; }
    
    [JsonPropertyName("carrier_short_name")]
    public string CarrierShortName { get; set; } = string.Empty;
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = string.Empty;
    
    [JsonPropertyName("expected")]
    public string Expected { get; set; } = string.Empty;
    
    [JsonPropertyName("is_apply_only")]
    public bool IsApplyOnly { get; set; }
    
    [JsonPropertyName("promotion_id")]
    public int PromotionId { get; set; }
    
    [JsonPropertyName("discount")]
    public decimal Discount { get; set; }
    
    [JsonPropertyName("weight_fee")]
    public decimal WeightFee { get; set; }
    
    [JsonPropertyName("location_first_fee")]
    public decimal LocationFirstFee { get; set; }
    
    [JsonPropertyName("location_step_fee")]
    public decimal LocationStepFee { get; set; }
    
    [JsonPropertyName("remote_area_fee")]
    public decimal RemoteAreaFee { get; set; }
    
    [JsonPropertyName("oil_fee")]
    public decimal OilFee { get; set; }
    
    [JsonPropertyName("location_fee")]
    public decimal LocationFee { get; set; }
    
    [JsonPropertyName("cod_fee")]
    public decimal CodFee { get; set; }
    
    [JsonPropertyName("service_fee")]
    public decimal ServiceFee { get; set; }
    
    [JsonPropertyName("total_fee")]
    public decimal TotalFee { get; set; }
    
    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }
    
    [JsonPropertyName("total_amount_carrier")]
    public decimal TotalAmountCarrier { get; set; }
    
    [JsonPropertyName("total_amount_shop")]
    public decimal TotalAmountShop { get; set; }
    
    [JsonPropertyName("price_table_id")]
    public int PriceTableId { get; set; }
    
    [JsonPropertyName("insurrance_fee")]
    public decimal InsurranceFee { get; set; }
    
    [JsonPropertyName("return_fee")]
    public decimal ReturnFee { get; set; }
}