namespace BLL.DTO.Courier;

public class RateResponseDTO
{
    public string Id { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public string? CarrierLogo { get; set; }
    public string CarrierShortName { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Expected { get; set; } = string.Empty;
    public bool IsApplyOnly { get; set; }
    public int PromotionId { get; set; }
    public decimal Discount { get; set; }
    public decimal WeightFee { get; set; }
    public decimal LocationFirstFee { get; set; }
    public decimal LocationStepFee { get; set; }
    public decimal RemoteAreaFee { get; set; }
    public decimal OilFee { get; set; }
    public decimal LocationFee { get; set; }
    public decimal CodFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalFee { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountCarrier { get; set; }
    public decimal TotalAmountShop { get; set; }
    public int PriceTableId { get; set; }
    public decimal InsurranceFee { get; set; }
    public decimal ReturnFee { get; set; }
}