namespace Infrastructure.Courier.Models;

public class OrderCreate
{
    public string OrderCode { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public string WardEncode { get; set; } = string.Empty;
    public string DistrictEncode { get; set; } = string.Empty;
    public int MainService { get; set; }
    public int Insurance { get; set; }
    public int CodFee { get; set; }
    public int StationDo { get; set; }
    public int StationPu { get; set; }
    public int Return { get; set; }
    public int R2s { get; set; }
    public int ReturnAgain { get; set; }
    public int Coupon { get; set; }
    public int DocumentReturn { get; set; }
    public int DoubleCheck { get; set; }
    public int DoubleCheckDeliver { get; set; }
    public int PickRemoteAreasFee { get; set; }
    public int DeliverRemoteAreasFee { get; set; }
    public int PickRemoteAreasFeeReturn { get; set; }
    public int DeliverRemoteAreasFeeReturn { get; set; }
    public int CodFailedFee { get; set; }
    public int TotalFee { get; set; }
    public DateTime ExpectedDeliveryTime { get; set; }
    public string OperationPartner { get; set; } = string.Empty;
}