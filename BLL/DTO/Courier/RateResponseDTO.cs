namespace BLL.DTO.Courier;

public class RateParentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RateReportDTO
{
    public decimal ScorePercent { get; set; }
    public decimal SuccessPercent { get; set; }
    public decimal ReturnPercent { get; set; }
    public int AvgTimeDelivery { get; set; }
    public string AvgTimeDeliveryFormat { get; set; } = string.Empty;
}

public class RateResponseDTO
{
    public int ServiceId { get; set; }
    public string ServiceMappingId { get; set; } = string.Empty;
    public string CarrierId { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public string CarrierShortName { get; set; } = string.Empty;
    public string? CarrierLogo { get; set; }
    public string? CarrierNote { get; set; }
    public string Service { get; set; } = string.Empty;
    public string ExpectedTxt { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public string HourApplyTxt { get; set; } = string.Empty;
    public bool IsApplyOnly { get; set; }
    public RateParentDTO? Parent { get; set; }
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
    public RateReportDTO? Report { get; set; }
}