namespace BLL.DTO.Address;

public class CourierCommuneResponseDTO
{
    public string CommuneCode { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
}