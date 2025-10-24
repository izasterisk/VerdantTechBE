using System.Text.Json.Serialization;

namespace Infrastructure.Courier.Models;

public class RateParent
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("short_name")]
    public string ShortName { get; set; } = string.Empty;
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class RateReport
{
    [JsonPropertyName("score_percent")]
    public decimal ScorePercent { get; set; }
    
    [JsonPropertyName("success_percent")]
    public decimal SuccessPercent { get; set; }
    
    [JsonPropertyName("return_percent")]
    public decimal ReturnPercent { get; set; }
    
    [JsonPropertyName("avg_time_delivery")]
    public int AvgTimeDelivery { get; set; }
    
    [JsonPropertyName("avg_time_delivery_format")]
    public string AvgTimeDeliveryFormat { get; set; } = string.Empty;
}

public class RateResponse
{
    [JsonPropertyName("service_id")]
    public int ServiceId { get; set; }
    
    [JsonPropertyName("service_mapping_id")]
    public string ServiceMappingId { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_id")]
    public string CarrierId { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_name")]
    public string CarrierName { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_short_name")]
    public string CarrierShortName { get; set; } = string.Empty;
    
    [JsonPropertyName("carrier_logo")]
    public string? CarrierLogo { get; set; }
    
    [JsonPropertyName("carrier_note")]
    public string? CarrierNote { get; set; }
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = string.Empty;
    
    [JsonPropertyName("expected_txt")]
    public string ExpectedTxt { get; set; } = string.Empty;
    
    [JsonPropertyName("service_description")]
    public string ServiceDescription { get; set; } = string.Empty;
    
    [JsonPropertyName("hour_apply_txt")]
    public string HourApplyTxt { get; set; } = string.Empty;
    
    [JsonPropertyName("is_apply_only")]
    public bool IsApplyOnly { get; set; }
    
    [JsonPropertyName("parent")]
    public RateParent? Parent { get; set; }
    
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
    
    [JsonPropertyName("report")]
    public RateReport? Report { get; set; }
}