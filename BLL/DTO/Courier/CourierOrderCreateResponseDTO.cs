namespace BLL.DTO.Courier;

public class CourierOrderCreateResponseDTO
{
    public string OrderCode { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public int TotalFee { get; set; }
    public DateTime ExpectedDeliveryTime { get; set; }
}