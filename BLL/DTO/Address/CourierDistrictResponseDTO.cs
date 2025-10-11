namespace BLL.DTO.Address;

public class CourierDistrictResponseDTO
{
    public int DistrictId { get; set; }
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}